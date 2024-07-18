﻿using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Queues;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Health;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Helpers
{
    public static class InfrastructureHealthCheckConstants
    {
        public const string CoreRequestGroup = "core";
        public const string WorkspacesRequestGroup = "workspaces";
        public const string AllRequestGroup = "all";
        public const string MainQueueRequestGroup = "0";
        public const string PoisonQueueRequestGroup = "1";

        public const string PoisonQueueSuffix = "-poison";

        public const string WorkspaceKeyCheck = "project-cmk";
        public const string CoreKeyCheck = "datahubportal-client-id";

        public const string TerraformAzureDatabricksResourceType = "terraform:azure-databricks";

        public const string DatahubStorageQueueConnectionStringConfigKey = "DatahubStorageQueue:ConnectionString";
        public const string DatahubServiceBusConnectionStringConfigKey = "DatahubServiceBus:ConnectionString";

        public const string AzureTenantIdEnvKey = "AZURE_TENANT_ID";
        public const string AzureClientIdEnvKey = "AZURE_CLIENT_ID";
        public const string AzureClientSecretEnvKey = "AZURE_CLIENT_SECRET";
    }

    public class HealthCheckHelper(DatahubProjectDBContext dbProjectContext,
        IProjectStorageConfigurationService projectStorageConfigurationService,
        AzureDevOpsConfiguration devopsConfig,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        private readonly DatahubProjectDBContext dbProjectContext = dbProjectContext;
        private readonly IProjectStorageConfigurationService projectStorageConfigurationService = projectStorageConfigurationService;
        private readonly AzureDevOpsConfiguration devopsConfig = devopsConfig;
        private readonly IConfiguration configuration = configuration;
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
        private readonly ILogger<HealthCheckHelper> logger = loggerFactory.CreateLogger<HealthCheckHelper>();

        /// <summary>
        /// Function that checks the health of an Azure SQL Database.
        /// </summary>
        /// <param name="request"></param>>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureSqlDatabase(InfrastructureHealthCheckMessage request)
        {
            // TODO: workspace specific databases

            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            bool connectable = await dbProjectContext.Database.CanConnectAsync();
            if (!connectable)
            {
                status = InfrastructureHealthStatus.Unhealthy;
                errors.Add("Cannot connect to the database.");
            }
            else
            {
                var test = await dbProjectContext.Projects.FirstOrDefaultAsync();
                if (test == null)
                {
                    status = InfrastructureHealthStatus.Degraded;
                    errors.Add("Cannot retrieve from the database.");
                }
            }

            return new(status, errors);
        }


        /// <summary>
        /// Function that gets the Azure Key Vault URL based on the request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The URL for the group given.</returns>
        private static Uri GetAzureKeyVaultUrl(InfrastructureHealthCheckMessage request) => request.Group == InfrastructureHealthCheckConstants.CoreRequestGroup ?
            new Uri($"https://{request.Name}.vault.azure.net/") :
            new Uri($"https://fsdh-proj-{request.Name}-dev-kv.vault.azure.net/");

        /// <summary>
        /// Function that checks the health of an Azure Key Vault.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureKeyVault(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            try
            {
                Environment.SetEnvironmentVariable(InfrastructureHealthCheckConstants.AzureTenantIdEnvKey, devopsConfig.TenantId);
                Environment.SetEnvironmentVariable(InfrastructureHealthCheckConstants.AzureClientIdEnvKey, devopsConfig.ClientId);
                Environment.SetEnvironmentVariable(InfrastructureHealthCheckConstants.AzureClientSecretEnvKey, devopsConfig.ClientSecret);

                var client =
                    new SecretClient(GetAzureKeyVaultUrl(request),
                        new DefaultAzureCredential()); // Authenticates with Azure AD and creates a SecretClient object for the specified key vault

                KeyVaultSecret secret;
                if (request.Group == InfrastructureHealthCheckConstants.CoreRequestGroup) // Key check for core
                {
                    secret = await client.GetSecretAsync(InfrastructureHealthCheckConstants.CoreKeyCheck);
                }
                else // Key check for workspaces (to verify)
                {
                    secret = await client.GetSecretAsync(InfrastructureHealthCheckConstants.WorkspaceKeyCheck);
                }

                try
                {
                    // Iterate through the keys in the key vault and check if they are expired
                    await foreach (var secretProperties in client.GetPropertiesOfSecretsAsync())
                    {
                        if (secretProperties.ExpiresOn < DateTime.UtcNow)
                        {
                            errors.Add($"The secret {secretProperties.Name} has expired.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add("Unable to retrieve the secrets from the key vault." + ex.GetType().ToString());
                    errors.Add($"Details: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                errors.Add("Unable to connect and retrieve a secret. " + ex.GetType().ToString());
                errors.Add($"Details: {ex.Message}");
            }

            if (errors.Count > 0)
            {
                status = InfrastructureHealthStatus.Unhealthy;
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that checks the health of an Azure Storage Account.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureStorageAccount(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            // Get the projects that match the request.Name
            try
            {
                string accountName = projectStorageConfigurationService.GetProjectStorageAccountName(request.Name);
                string accountKey = await projectStorageConfigurationService.GetProjectStorageAccountKey(request.Name);

                var projectStorageManager = new AzureCloudStorageManager(accountName, accountKey);

                if (projectStorageManager is null)
                {
                    status = InfrastructureHealthStatus.Degraded;
                    errors.Add("Unable to find the data container.");
                }
            }
            catch (Exception ex)
            {
                status = InfrastructureHealthStatus.Unhealthy;
                errors.Add("Unable to retrieve project. " + ex.GetType().ToString());
                errors.Add($"Details: {ex.Message}");
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that checks the health of an Azure Databricks workspace, ACL and cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureDatabricksHealth(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            var project = await dbProjectContext.Projects
                .AsNoTracking()
                .Include(p => p.Resources)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == request.Name);

            // If the project is null, the project does not exist or there was an error retrieving it
            if (project == null)
            {
                status = InfrastructureHealthStatus.Unhealthy;
                errors.Add("Failed to retrieve project.");
            }
            else
            {
                // We check if the project has a databricks resource. If not, we return a create status.
                var hasDatabricksResource = project.Resources.Any(r => r.ResourceType == InfrastructureHealthCheckConstants.TerraformAzureDatabricksResourceType);

                if (!hasDatabricksResource)
                {
                    status = InfrastructureHealthStatus.Create;
                }
                else
                {
                    // We attempt to retrieve the databricks URL. If we cannot, we return an unhealthy status.
                    var databricksUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project);

                    if (databricksUrl == null)
                    {
                        status = InfrastructureHealthStatus.Unhealthy;
                        errors.Add("Failed to retrieve Databricks URL.");
                    }
                    else
                    {
                        try
                        {
                            var azureDevOpsClient = new AzureDevOpsClient(devopsConfig);
                            var accessToken = await azureDevOpsClient.AccessTokenAsync();

                            var databricksClient = new DatabricksClientUtils(databricksUrl, accessToken.Token);

                            // Verify Instance Availability
                            var instanceRunning = await databricksClient.IsDatabricksInstanceRunning();
                            if (!instanceRunning)
                            {
                                status = InfrastructureHealthStatus.Unhealthy;
                                errors.Add("Databricks instance is not available.");
                            }
                            else
                            {
                                // Verify ACL Status
                                var aclStatus = await databricksClient.VerifyACLStatus();
                                if (!aclStatus)
                                {
                                    status = InfrastructureHealthStatus.Unhealthy;
                                    errors.Add("Failed to verify ACL status.");
                                }
                                else
                                {
                                    // Check Cluster Status
                                    var clusterStatus = await databricksClient.GetClusterStatus("");
                                    if (string.IsNullOrEmpty(clusterStatus) || clusterStatus != "Running")
                                    {
                                        status = InfrastructureHealthStatus.Unhealthy;
                                        errors.Add("Cluster is not in the running state.");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            status = InfrastructureHealthStatus.Unhealthy;
                            errors.Add($"Error while checking Databricks health: {ex.Message}");
                        }
                    }
                }
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that checks the health of the Azure Function App.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureFunctions(InfrastructureHealthCheckMessage request)
        {
            var azureFunctionUrl = $"http://{request.Name}/api/FunctionsHealthCheck";
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            try
            {
                using var httpClient = httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(azureFunctionUrl);

                if (!response.IsSuccessStatusCode)
                {
                    errors.Add($"Azure Function returned an unhealthy status code: {response.StatusCode}.");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error while checking Azure Function health: {ex.Message}");
            }

            if (errors.Count > 0)
            {
                status = InfrastructureHealthStatus.Unhealthy;
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that returns the queue name for the given request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>[name]-poison if the request group is "1" (poison queue), or [name] otherwise</returns>
        public static string GetRequestQueueName(InfrastructureHealthCheckMessage request) => request.Group == InfrastructureHealthCheckConstants.PoisonQueueRequestGroup ?
            request.Name + InfrastructureHealthCheckConstants.PoisonQueueSuffix :
            request.Name;

        /// <summary>
        /// Function that checks the health of an Azure Storage Queue. Group == 1 for poison queue.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureStorageQueue(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            var storageConnectionString = configuration[InfrastructureHealthCheckConstants.DatahubStorageQueueConnectionStringConfigKey];

            string queueName = GetRequestQueueName(request);

            try
            {
                QueueClient queueClient = new(storageConnectionString, queueName);

                if (queueClient is null)
                {
                    errors.Add("Unable to connect to the queue.");
                }
                else
                {
                    bool queueExists = await queueClient.ExistsAsync();
                    if (!queueExists)
                    {
                        errors.Add("The queue does not exist.");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error while checking Azure Storage Queue: {ex.Message}");
            }

            if (errors.Count > 0)
            {
                status = InfrastructureHealthStatus.Unhealthy;
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that checks the health of the Azure Service Bus
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureServiceBusQueue(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            var serviceBusConnectionString = configuration[InfrastructureHealthCheckConstants.DatahubServiceBusConnectionStringConfigKey];

            string queueName = GetRequestQueueName(request);

            try
            {
                ServiceBusClient serviceBusClient = new(serviceBusConnectionString);
                ServiceBusReceiver receiver = serviceBusClient.CreateReceiver(queueName);

                if (receiver is null)
                {
                    errors.Add("Unable to connect to the queue.");
                }
                else
                {
                    // attempt to read message to check if queue exists; receiver is created with no errors for non-existing queue
                    ServiceBusReceivedMessage message = await receiver.PeekMessageAsync();
                    if (message != null && request.Group == InfrastructureHealthCheckConstants.PoisonQueueRequestGroup)
                    {
                        if (string.IsNullOrEmpty(message.DeadLetterReason))
                        {
                            errors.Add("Dead letter reason is empty.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error while checking Azure Service Bus Queue: {ex.Message.Replace(",", ".")}");
            }

            if (errors.Count > 0)
            {
                status = InfrastructureHealthStatus.Unhealthy;
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that checks the health of the Azure Web App, if enabled.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckWebApp(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            var project = await dbProjectContext.Projects
                .AsNoTracking()
                .Include(p => p.Resources)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == request.Name);

            // If the project is null, the project does not exist or there was an error retrieving it
            if (project == null)
            {
                errors.Add("Unable to retrieve project.");
                status = InfrastructureHealthStatus.Create;
            }
            else
            {
                // We check if the project has a web app resource. If not, we return a create status.
                if (project.WebAppEnabled == null || project.WebAppEnabled == false)
                {
                    status = InfrastructureHealthStatus.Create;
                }
                else
                {
                    string url = project.WebApp_URL;

                    // Validate if the URL is valid
                    if (!Uri.TryCreate(url, UriKind.Absolute, out var result))
                    {
                        status = InfrastructureHealthStatus.Unhealthy;
                        errors.Add("Invalid Web App URL.");
                        if (!string.IsNullOrEmpty(url) && !url.ToLower().StartsWith("http"))
                        {
                            url = "https://" + url;  // add https if not present
                        }
                    }

                    try
                    {
                        // We attempt to connect to the URL. If we cannot, we return an unhealthy status.
                        using var httpClient = httpClientFactory.CreateClient();
                        var response = await httpClient.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                        {
                            status = InfrastructureHealthStatus.Unhealthy;
                            errors.Add($"Web App returned an unhealthy status code: {response.StatusCode}. {response.ReasonPhrase}");
                        }
                        else
                        {
                            var content = await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        status = InfrastructureHealthStatus.Unhealthy;
                        errors.Add($"Error while checking Web App health: {ex.Message}");
                    }
                }
            }

            return new(status, errors);
        }

        /// <summary>
        /// For queue-based checks, updates the name depending on whether the request is for the poison queue.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The correct health check name</returns>
        private static string GenerateHealthCheckName(InfrastructureHealthCheckMessage request) => request.Type switch
        {
            InfrastructureHealthResourceType.AzureStorageQueue => GetRequestQueueName(request),
            InfrastructureHealthResourceType.AsureServiceBus => GetRequestQueueName(request),
            _ => request.Name
        };

        /// <summary>
        /// Checks the infrastructure health of a specific resource.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An InfrastructureHealthCheckResponse, containing InfrastructureHealthCheck record and list of errors.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<InfrastructureHealthCheckResponse2> RunHealthCheck(InfrastructureHealthCheckMessage request)
        {
            var intermediateResult = request?.Type switch
            {
                InfrastructureHealthResourceType.AzureSqlDatabase => await CheckAzureSqlDatabase(request),
                InfrastructureHealthResourceType.AzureStorageAccount => await CheckAzureStorageAccount(request),
                InfrastructureHealthResourceType.AzureKeyVault => await CheckAzureKeyVault(request),
                InfrastructureHealthResourceType.AzureDatabricks => await CheckAzureDatabricksHealth(request),
                InfrastructureHealthResourceType.AzureStorageQueue => await CheckAzureStorageQueue(request),
                InfrastructureHealthResourceType.AsureServiceBus => await CheckAzureServiceBusQueue(request),
                InfrastructureHealthResourceType.AzureWebApp => await CheckWebApp(request),
                InfrastructureHealthResourceType.AzureFunction => await CheckAzureFunctions(request),
                _ => throw new InvalidOperationException()
            };

            var details = intermediateResult.Errors.Count == 0 ?
                (intermediateResult.Status == InfrastructureHealthStatus.Healthy ? default : "Please investigate.") :
                string.Join("; ", intermediateResult.Errors);

            var result = new InfrastructureHealthCheck()
            {
                Group = request.Group,
                Name = GenerateHealthCheckName(request),
                ResourceType = request.Type,
                Status = intermediateResult.Status,
                HealthCheckTimeUtc = DateTime.UtcNow,
                Details = details,
            };

            return new(result, intermediateResult.Errors);
        }

        /// <summary>
        /// Function that runs all infrastructure health checks.
        /// </summary>
        /// <returns>Results of all health checks</returns>
        public async Task<IEnumerable<InfrastructureHealthCheckResponse2>> RunAllChecks()
        {
            var coreChecks = new List<InfrastructureHealthCheckMessage>()
            {
                new(InfrastructureHealthResourceType.AzureSqlDatabase, InfrastructureHealthCheckConstants.CoreRequestGroup, InfrastructureHealthCheckConstants.CoreRequestGroup),
                new(InfrastructureHealthResourceType.AzureKeyVault, InfrastructureHealthCheckConstants.CoreRequestGroup, InfrastructureHealthCheckConstants.CoreRequestGroup),
                new(InfrastructureHealthResourceType.AzureKeyVault, InfrastructureHealthCheckConstants.WorkspacesRequestGroup, InfrastructureHealthCheckConstants.WorkspacesRequestGroup),
                new(InfrastructureHealthResourceType.AzureFunction, InfrastructureHealthCheckConstants.CoreRequestGroup, "localhost:7071")
            };

            // TODO exclude these if AzureSqlDatabase check fails
            var projects = await dbProjectContext.Projects
                .AsNoTracking()
                .ToListAsync();
            var workspaceChecks = projects.SelectMany(p => new List<InfrastructureHealthCheckMessage>()
            {
                new(InfrastructureHealthResourceType.AzureSqlDatabase, InfrastructureHealthCheckConstants.WorkspacesRequestGroup, p.Project_Acronym_CD),
                new(InfrastructureHealthResourceType.AzureStorageAccount, InfrastructureHealthCheckConstants.WorkspacesRequestGroup, p.Project_Acronym_CD),
                new(InfrastructureHealthResourceType.AzureDatabricks, InfrastructureHealthCheckConstants.WorkspacesRequestGroup, p.Project_Acronym_CD),
                new(InfrastructureHealthResourceType.AzureWebApp, InfrastructureHealthCheckConstants.WorkspacesRequestGroup, p.Project_Acronym_CD)
            });

            string[] queuesToCheck =
            [
                "delete-run-request", "email-notification", "pong-queue", "project-capacity-update",
                "project-inactivity-notification", "project-usage-notification",
                "project-usage-update", "resource-run-request", "storage-capacity", "terraform-output",
                "user-inactivity-notification", "user-run-request"
            ];
            var queueChecks = queuesToCheck.SelectMany(q => new List<InfrastructureHealthCheckMessage>()
            {
                new(InfrastructureHealthResourceType.AzureStorageQueue, InfrastructureHealthCheckConstants.MainQueueRequestGroup, q),
                new(InfrastructureHealthResourceType.AzureStorageQueue, InfrastructureHealthCheckConstants.PoisonQueueSuffix, q)
            });

            var allChecks = coreChecks.Concat(workspaceChecks).Concat(queueChecks);

            var results = await Task.WhenAll(allChecks.Select(RunHealthCheck));

            return results;
        }

        private static InfrastructureHealthCheck CloneWithoutId(InfrastructureHealthCheck healthCheck) => new()
        {
            Details = healthCheck.Details,
            Group = healthCheck.Group,
            HealthCheckTimeUtc = healthCheck.HealthCheckTimeUtc,
            Name = healthCheck.Name,
            ResourceType = healthCheck.ResourceType,
            Status = healthCheck.Status,
            Url = healthCheck.Url,
        };

        public async Task StoreHealthCheck(InfrastructureHealthCheck check)
        {
            if (string.IsNullOrEmpty(check.Name) || string.IsNullOrEmpty(check.Group))
            {
                logger.LogWarning("Got a health check with empty identifier");
                return;
            }

            var existingChecks = await dbProjectContext.InfrastructureHealthChecks
                .Where(c => c.Group == check.Group && c.Name == check.Name && c.ResourceType == check.ResourceType)
                .ToListAsync();

            if (existingChecks?.Count > 0)
            {
                dbProjectContext.InfrastructureHealthChecks.RemoveRange(existingChecks);
            }

            dbProjectContext.InfrastructureHealthChecks.Add(CloneWithoutId(check));

            try
            {
                await dbProjectContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error saving health check (type: {check.ResourceType}; group: {check.Group}; name: {check.Name})");
            }

        }

        public async Task StoreHealthCheckRun(InfrastructureHealthCheck check)
        {
            if (string.IsNullOrEmpty(check.Name) || string.IsNullOrEmpty(check.Group))
            {
                logger.LogWarning("Got a health check run with empty identifier");
                return;
            }

            dbProjectContext.InfrastructureHealthCheckRuns.Add(CloneWithoutId(check));

            try
            {
                await dbProjectContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error saving health check run (type: {check.ResourceType}; group: {check.Group}; name: {check.Name})");
            }
        }
    }

    public record IntermediateHealthCheckResult(InfrastructureHealthStatus Status, List<string> Errors);
    public record InfrastructureHealthCheckResponse2(InfrastructureHealthCheck Check, List<string>? Errors);
}
