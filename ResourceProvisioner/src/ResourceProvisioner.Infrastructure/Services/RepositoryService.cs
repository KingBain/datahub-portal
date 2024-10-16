using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Datahub.Shared.Clients;
using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Enums;
using ResourceProvisioner.Domain.Events;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Domain.Messages;
using ResourceProvisioner.Domain.ValueObjects;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Polly;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Common;
using Version = System.Version;

namespace ResourceProvisioner.Infrastructure.Services;

public partial class RepositoryService : IRepositoryService
{
    /// <summary>
    /// Retrieves the regular expression used for matching module versions in the module repository.
    /// The regular expression pattern matches the directory structure of the module repository versions in the format vX.Y.Z.
    /// </summary>
    /// <returns>The regular expression pattern for matching module versions.</returns>
    [GeneratedRegex(@"(/|\\)v\d+\.\d+\.\d+$")]
    private static partial Regex ModuleRegex();

    private readonly ILogger<RepositoryService> _logger;
    private readonly ITerraformService _terraformService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ResourceProvisionerConfiguration _resourceProvisionerConfiguration;

    public RepositoryService(IHttpClientFactory httpClientFactory, ILogger<RepositoryService> logger,
        ResourceProvisionerConfiguration resourceProvisionerConfiguration,
        ITerraformService terraformService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _resourceProvisionerConfiguration = resourceProvisionerConfiguration;
        _terraformService = terraformService;

    }

    public async Task<PullRequestUpdateMessage> HandleResourcing(CreateResourceRunCommand command)
    {
        var user = command.RequestingUserEmail ?? throw new NullReferenceException("Requesting user's email is null");
        _logger.LogInformation("Checking out workspace branch for {WorkspaceAcronym}", command.Workspace.Acronym);
        await FetchRepositoriesAndCheckoutProjectBranch(command.Workspace.Acronym!);

        _logger.LogInformation(
            "Executing the following resource runs in workspace {WorkspaceAcronym} for user {User}: [{ResourceRuns}]",
            command.Workspace.Acronym, user, string.Join(", ", command.Templates.Select(x => x.Name)));
        var repositoryUpdateEvents =
            await ExecuteResourceRuns(command.Templates, command.Workspace, user);

        _logger.LogInformation("Pushing changes to remote repository for {WorkspaceAcronym}",
            command.Workspace.Acronym);
        await PushInfrastructureRepository(command.Workspace.Acronym!);

        _logger.LogInformation("Creating pull request for {WorkspaceAcronym}", command.Workspace.Acronym);
        var pullRequestValueObject =
            await CreateInfrastructurePullRequest(command.Workspace.Acronym!, user);

        _logger.LogInformation("Completing pull request for {WorkspaceAcronym}", command.Workspace.Acronym);
        await AutoApproveInfrastructurePullRequest(pullRequestValueObject.PullRequestId, command.Workspace.Acronym!);

        var pullRequestMessage = new PullRequestUpdateMessage
        {
            PullRequestValueObject = pullRequestValueObject,
            TerraformWorkspace = command.Workspace,
            Events = repositoryUpdateEvents
        };

        if (pullRequestMessage.Events.All(x => x.StatusCode != MessageStatusCode.Error))
        {
            return pullRequestMessage;
        }

        pullRequestMessage.Events
            .Where(x => x.StatusCode == MessageStatusCode.Error)
            .ToList()
            .ForEach(x => _logger.LogError(x.Message, x));
        throw new Exception("Error while handling resource run request");
    }

    /// <summary>
    /// Retrieves a list of module versions from the module repository.
    ///
    /// Note: That the directory structure of the module repository must be in the format vX.Y.Z for it to be recognized as a version.
    /// </summary>
    /// <returns>A list of <see cref="Version"/> representing the available module versions.</returns>
    public async Task<List<Version>> GetModuleVersions()
    {
        var repositoryPath = DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration);
        var modulePath = Path.Combine(repositoryPath,
            _resourceProvisionerConfiguration.ModuleRepository.ModulePathPrefix);

        // check if module path exists
        if (!Directory.Exists(modulePath))
        {
            _logger.LogInformation("Module path {ModulePath} does not exist, fetching module repository", modulePath);
            await FetchModuleRepository();
        }

        var versions = Directory.GetDirectories(modulePath)
            .Where(x => ModuleRegex().IsMatch(x))
            .Select(x =>
                new Version(x
                        .Replace('/', Path.DirectorySeparatorChar)
                        .Split(Path.DirectorySeparatorChar)
                        .Last()[1..] // remove the v prefix
                ))
            .ToList();
        return versions;
    }

    public Task FetchModuleRepository()
    {
        var repositoryUrl = _resourceProvisionerConfiguration.ModuleRepository.Url;
        var localPath = _resourceProvisionerConfiguration.ModuleRepository.LocalPath;

        _logger.LogInformation("Fetching repository {RepositoryUrl} to {LocalPath}", repositoryUrl, localPath);
        var repositoryPath = DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration);
        DirectoryUtils.VerifyDirectoryDoesNotExist(repositoryPath);

        _logger.LogInformation("Cloning repository {RepositoryUrl} to {LocalPath}", repositoryUrl, repositoryPath);
        Repository.Clone(repositoryUrl, repositoryPath);

        if (_resourceProvisionerConfiguration.ModuleRepository.Branch != ModuleRepositoryConfiguration.DefaultBranch)
        {
            using var repo = new Repository(repositoryPath);
            var branch =
                repo.Branches[$"refs/remotes/origin/{_resourceProvisionerConfiguration.ModuleRepository.Branch}"];
            if (branch == null)
            {
                _logger.LogInformation("Branch {Branch} does not exist, checking out default branch",
                    _resourceProvisionerConfiguration.ModuleRepository.Branch);
                branch = repo.Branches[ModuleRepositoryConfiguration.DefaultBranch];
            }

            Commands.Checkout(repo, branch);
        }

        _logger.LogInformation("Repository {RepositoryUrl} cloned to {LocalPath}", repositoryUrl, repositoryPath);
        return Task.CompletedTask;
    }

    public async Task FetchInfrastructureRepository()
    {
        var localPath = _resourceProvisionerConfiguration.InfrastructureRepository.LocalPath;
        var repositoryUrl = _resourceProvisionerConfiguration.InfrastructureRepository.Url;
        _logger.LogInformation("Fetching repository {RepositoryUrl} to {LocalPath}", repositoryUrl, localPath);
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);
        DirectoryUtils.VerifyDirectoryDoesNotExist(repositoryPath);

        var azureDevOpsClient =
            new AzureDevOpsClient(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration);
        var accessToken = await azureDevOpsClient.AccessTokenAsync();

        var cloneOptions = new CloneOptions
        {
            FetchOptions =
            {
                CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
                {
                    Username = _resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration
                        .ClientId,
                    Password = accessToken.Token
                }
            }
        };

        _logger.LogInformation("Cloning repository {RepositoryUrl} to {LocalPath}", repositoryUrl, repositoryPath);
        Repository.Clone(repositoryUrl, repositoryPath, cloneOptions);

        _logger.LogInformation("Repository {RepositoryUrl} cloned to {LocalPath}", repositoryUrl, repositoryPath);
    }

    public async Task CheckoutInfrastructureBranch(string workspaceName)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);
        _logger.LogInformation("Checking out branch {WorkspaceName} in {Path}", workspaceName, repositoryPath);
        using var repo = new Repository(repositoryPath);
        var branch = repo.Branches[workspaceName];
        if (branch == null)
        {
            _logger.LogInformation("Branch {WorkspaceName} does not exist in {Path}, creating it now", workspaceName,
                repositoryPath);
            branch = repo.CreateBranch(workspaceName);
        }

        Commands.Checkout(repo, branch);

        _logger.LogInformation("Branch {WorkspaceName} checked out in {Path}", workspaceName, repositoryPath);

        _logger.LogInformation("Checking upstream for any updates in branch");

        var azureDevOpsClient =
            new AzureDevOpsClient(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration);
        var accessToken = await azureDevOpsClient.AccessTokenAsync();

        var pullOptions = new PullOptions()
        {
            FetchOptions = new FetchOptions
            {
                CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
                {
                    Username = _resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration
                        .ClientId,
                    Password = accessToken.Token
                }
            }
        };

        var signature = new Signature(new Identity("Auto-merge", "Auto-merge"), DateTimeOffset.Now);
        try
        {
            var remote = repo.Network.Remotes["origin"];
            repo.Branches.Update(branch, b => b.Remote = remote.Name, b => b.UpstreamBranch = branch.CanonicalName);
            Commands.Pull(repo, signature, pullOptions);
        }
        catch (MergeFetchHeadNotFoundException)
        {
            _logger.LogInformation("No upstream updates found");
        }
    }

    public Task CommitTerraformTemplate(TerraformTemplate template, string username)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);

        _logger.LogInformation("Committing changes in {LocalPath}", repositoryPath);
        using var repository = new Repository(repositoryPath);

        _logger.LogInformation("Adding all files in {LocalPath}", repositoryPath);
        Commands.Stage(repository, "*");

        var author = new Signature(username, username, DateTimeOffset.Now);
        _logger.LogInformation(
            "Committing all files in {LocalPath} for module {ModuleName} as {Author}", repositoryPath,
            template.Name, author);
        try
        {
            repository.Commit($"Committing {template.Name} changes", author, author);
            _logger.LogInformation("Changes committed in {LocalPath}", repositoryPath);
        }
        catch (EmptyCommitException e)
        {
            _logger.LogInformation(e, "No changes to commit in {LocalPath}", repositoryPath);
            throw new NoChangesDetectedException($"No changes detected after adding {template.Name} to project");
        }

        return Task.CompletedTask;
    }

    public async Task PushInfrastructureRepository(string workspaceAcronym)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);

        var azureDevOpsClient =
            new AzureDevOpsClient(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration);
        var accessToken = await azureDevOpsClient.AccessTokenAsync();
        var options = new PushOptions
        {
            CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
            {
                Username = _resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientId,
                Password = accessToken.Token
            },
        };

        using var repo = new Repository(repositoryPath);
        var branch = repo.Branches[workspaceAcronym];
        var remote = repo.Network.Remotes["origin"];
        repo.Branches.Update(branch, b => b.Remote = remote.Name, b => b.UpstreamBranch = branch.CanonicalName);

        _logger.LogInformation("Pushing changes in {LocalPath} to {Branch} branch", repositoryPath,
            branch.CanonicalName);

        await Task.Run(() => repo.Network.Push(repo.Branches[workspaceAcronym], options));

        _logger.LogInformation("Changes pushed in {LocalPath} to {Branch} branch", repositoryPath,
            branch.CanonicalName);
    }

    public async Task<PullRequestValueObject> CreateInfrastructurePullRequest(string workspaceAcronym, string username)
    {
        // create a pull request in Azure DevOps
        _logger.LogInformation("Creating infrastructure pull request");
        var postBody = BuildPullRequestPostBody(workspaceAcronym);

        var postUrl =
            $"{_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}?api-version={_resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";

        _logger.LogInformation("Posting infrastructure pull request to {Url}", postUrl);
        var httpClient = _httpClientFactory.CreateClient("InfrastructureHttpClient");
        var response = await httpClient.PostAsync(postUrl, postBody);

        // get the pull request id
        _logger.LogInformation("Getting infrastructure pull request url");
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonNode>(content);

        var pullRequestId =
            data?["pullRequestId"]?.ToString();

        // TODO: Test this!
        if (string.IsNullOrWhiteSpace(pullRequestId))
        {
            if (data?["typeKey"]?.ToString() == "GitPullRequestExistsException")
            {
                pullRequestId = await GetExistingPullRequestId(workspaceAcronym);
            }
            else
            {
                throw new Exception($"Could not get pull request id for {workspaceAcronym}");
            }
        }

        var pullRequestUrl = BuildPullRequestUrl(pullRequestId);
        _logger.LogInformation("Infrastructure pull request url is {PullRequestUrl}", pullRequestUrl);

        return new PullRequestValueObject(workspaceAcronym, pullRequestUrl, int.Parse(pullRequestId));
    }

    public async Task AutoApproveInfrastructurePullRequest(int pullRequestId, string workspaceAcronym)
    {
        var patchContent = BuildPullRequestPatchBody(workspaceAcronym);
        var patchUrl =
            $"{_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}/{pullRequestId}?api-version={_resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";

        const int retryAmount = 5;
        var retryPolicy = Policy
            .Handle<AutoApproveIncompleteException>()
            .WaitAndRetryAsync(retryAmount, retryAttempt =>
                TimeSpan.FromSeconds(1),
                (exception, _, _, _) =>
                {
                    _logger.LogWarning(exception, "Auto-approve infrastructure pull request failed, retrying");
                });

        await retryPolicy.ExecuteAsync(async ct =>
        {
            await SendAutoApprovePatchRequestAsync(patchUrl, patchContent);
        }, CancellationToken.None);
    }
    
    public async Task SendAutoApprovePatchRequestAsync(string patchUrl, StringContent patchContent)
    {
        _logger.LogInformation("Patching auto-approve infrastructure pull request to {Url}", patchUrl);
        var httpClient = _httpClientFactory.CreateClient("InfrastructureHttpClient");
        var response = await httpClient.PatchAsync(patchUrl, patchContent);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Could not auto-approve infrastructure pull request {PullRequestUrl}", patchUrl);
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error: {Error}", content);
            throw new AutoApproveException($"Could not auto-approve infrastructure pull request {patchUrl}");
        }
        
        _logger.LogInformation("Infrastructure pull request {PullRequestUrl} auto-approved", patchUrl);
        
        // Check that the json content of the response has an object "closedBy"
        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonContent = JsonSerializer.Deserialize<JsonNode>(responseContent);
        if (jsonContent?["closedBy"] is null)
        {
            _logger.LogError("Infrastructure pull request {PullRequestUrl} was not auto-approved", patchUrl);
            throw new AutoApproveIncompleteException($"Infrastructure pull request {patchUrl} was not auto-approved");
        }
    }

    private StringContent BuildPullRequestPatchBody(string workspaceAcronym)
    {
        _logger.LogInformation(
            "Building infrastructure pull request patch body for complete by user {ClientId}",
            _resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientId);
        var patchData = new JsonObject
        {
            ["status"] = "completed",
            ["lastMergeSourceCommit"] = new JsonObject
            {
                ["commitId"] = GetBranchLastCommitId(workspaceAcronym)
            },
            ["completionOptions"] = new JsonObject
            {
                ["deleteSourceBranch"] = false,
                ["mergeCommitMessage"] = "Auto-merged by ResourceProvisioner"
            }
        };
        var patchBody = new StringContent(JsonSerializer.Serialize(patchData), Encoding.UTF8, "application/json");
        return patchBody;
    }

    public virtual string GetBranchLastCommitId(string branchName)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);
        using var repo = new Repository(repositoryPath);
        var branch = repo.Branches[branchName];

        if (branch is null)
        {
            _logger.LogError("Branch {BranchName} does not exist in {RepositoryPath}", branchName, repositoryPath);
            throw new NullReferenceException($"Branch {branchName} does not exist in {repositoryPath}");
        }

        return branch.Tip.Sha;
    }

    private string BuildPullRequestUrl(string pullRequestId)
    {
        return $"{_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestBrowserUrl}/{pullRequestId}";
    }

    private StringContent BuildPullRequestPostBody(string workspaceAcronym)
    {
        var postData = new JsonObject
        {
            ["sourceRefName"] = $"refs/heads/{workspaceAcronym}",
            ["targetRefName"] = $"refs/heads/{_resourceProvisionerConfiguration.InfrastructureRepository.MainBranch}",
            ["title"] = $"[{workspaceAcronym}] Infrastructure changes",
            ["description"] = $"[{workspaceAcronym}] Infrastructure changes",
        };
        var postBody = new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json");
        return postBody;
    }

    public async Task FetchRepositoriesAndCheckoutProjectBranch(string workspaceAcronym)
    {
        await FetchModuleRepository();
        await FetchInfrastructureRepository();
        await CheckoutInfrastructureBranch(workspaceAcronym);
    }

    public async Task<List<RepositoryUpdateEvent>> ExecuteResourceRuns(List<TerraformTemplate> modules,
        TerraformWorkspace terraformWorkspace, string requestingUsername)
    {
        var repositoryUpdateEvents = new List<RepositoryUpdateEvent>();

        await ValidateWorkspaceVersion(terraformWorkspace);
        
        // Execute each module but make sure the `new-project-template` module is first
        modules = modules.OrderBy(x => x.Name != TerraformTemplate.NewProjectTemplate).ToList();

        foreach (var module in modules)
        {
            var result = await ExecuteResourceRun(module, terraformWorkspace, requestingUsername);
            repositoryUpdateEvents.Add(result);
        }

        return repositoryUpdateEvents;
    }

    public async Task ValidateWorkspaceVersion(TerraformWorkspace terraformWorkspace)
    {
        // Old behavior, to maintain existing versions
        // if (terraformWorkspace.Version == TerraformWorkspace.DefaultVersion)
        // {
        //     var versions = await GetModuleVersions();
        //     var latestVersion = versions.Max();
        //     terraformWorkspace.Version = $"v{latestVersion!.ToString()}";
        // }

        // new behavior to always update the version
        var versions = await GetModuleVersions();
        var latestVersion = versions.Max();
        terraformWorkspace.Version = $"v{latestVersion!.ToString()}";
    }

    public async Task<RepositoryUpdateEvent> ExecuteResourceRun(TerraformTemplate template,
        TerraformWorkspace terraformWorkspace,
        string requestingUsername)
    {
        try
        {
            await _terraformService.CopyTemplateAsync(template.Name, terraformWorkspace);
            await _terraformService.ExtractVariables(template.Name, terraformWorkspace);
            switch (template.Name)
            {
                case TerraformTemplate.NewProjectTemplate:
                    await _terraformService.ExtractBackendConfig(terraformWorkspace.Acronym!);
                    break;
                case TerraformTemplate.VariableUpdate:
                    await _terraformService.ExtractAllVariables(terraformWorkspace);
                    break;
            }

            await CommitTerraformTemplate(template, requestingUsername);

            return new RepositoryUpdateEvent()
            {
                Message =
                    $"Successfully created resource run for [{terraformWorkspace.Version}]{template.Name} in {terraformWorkspace.Acronym}",
                StatusCode = MessageStatusCode.Success
            };
        }
        catch (NoChangesDetectedException)
        {
            return new RepositoryUpdateEvent()
            {
                Message =
                    $"No changes detected after resource run for [{terraformWorkspace.Version}]{template.Name} in {terraformWorkspace.Acronym}",
                StatusCode = MessageStatusCode.NoChangesDetected
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error while creating resource run for [{ModuleVersion}]{ModuleName} in {WorkspaceAcronym}",
                terraformWorkspace.Version, template.Name, terraformWorkspace.Acronym);

            return new RepositoryUpdateEvent()
            {
                Message =
                    $"Error creating resource run for [{terraformWorkspace.Version}]{template.Name} in {terraformWorkspace.Acronym}",
                StatusCode = MessageStatusCode.Error
            };
        }
    }


    private async Task<string> GetExistingPullRequestId(string workspaceAcronym)
    {
        _logger.LogInformation("Pull request already exists, fetching pull request id");
        var url =
            $"{_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}?searchCriteria.status=active&searchCriteria.sourceRefName=refs/heads/{workspaceAcronym}&api-version={_resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";

        using var httpClient = _httpClientFactory.CreateClient("InfrastructureHttpClient");
        var response = await httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonNode>(content);

        return data?["value"]?
                   .AsArray()
                   .FirstOrDefault(node => node?["sourceRefName"]?.ToString() == $"refs/heads/{workspaceAcronym}")?
                   .AsObject()["pullRequestId"]?.ToString() ??
               throw new NullReferenceException(
                   $"Could not get existing pull request id for workspace {workspaceAcronym}");
    }
}