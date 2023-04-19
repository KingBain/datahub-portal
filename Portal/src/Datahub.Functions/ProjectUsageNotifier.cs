using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Storage.Queues.Models;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class ProjectUsageNotifier
    {
        private readonly IMediator _mediator;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly int[] _notificationPercents;
        private readonly ILogger<ProjectUsageNotifier> _logger;
        private readonly QueuePongService _pongService;

        public ProjectUsageNotifier(ILoggerFactory loggerFactory, AzureConfig config, IMediator mediator, 
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, QueuePongService pongService)
        {
            _logger = loggerFactory.CreateLogger<ProjectUsageNotifier>();
            _mediator = mediator;
            _dbContextFactory = dbContextFactory;
            _notificationPercents = ParseNotificationPercents(config.NotificationPercents ?? "25,50,80,100");
            _pongService = pongService;
        }

        [Function("ProjectUsageNotifier")]
        public async Task Run([QueueTrigger("%QueueProjectUsageNotification%", Connection = "DatahubStorageConnectionString")] string queueItem, 
            CancellationToken cancellationToken)
        {
            // test for ping
            if (await _pongService.Pong(queueItem))
                return;

            // deserialize message
            var message = DeserializeQueueMessage(queueItem);

            // verify message 
            if (message is null || message.ProjectId <= 0)
            {
                throw new Exception($"Invalid queue message:\n{queueItem}");
            }

            // run project verification
            await VerifyAndNotifyProject(message.ProjectId, cancellationToken);
        }

        private async Task VerifyAndNotifyProject(int projectId, CancellationToken cancellationToken)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            // load from details from db
            var details = await GetProjectDetails(ctx, projectId, cancellationToken);

            if (details?.Credits is null)
            {
                // log that details credits are null
                _logger.LogWarning("Project {ProjectId} details or credits are null", projectId);
                return;
            }

            // calc current %
            var currentPercent = details.ProjectBudget > 0 ? (int)Math.Round(100.0 * details.Credits.Current / details.ProjectBudget) : 0;
            
            // get the matching notification %
            var notificationPerc = GetNotificationPercent(currentPercent);
                        
            // check if notification is not needed
            if (notificationPerc == 0 || details.Credits.PercNotified == notificationPerc)
            {
                return;
            }

            try
            {
                var notificationEmail = GetNotificationEmail(notificationPerc, details.Contacts);
                await _mediator.Send(notificationEmail, cancellationToken);

                details.Credits.PercNotified = notificationPerc;
                details.Credits.LastNotified = DateTime.UtcNow;

                ctx.Project_Credits.Update(details.Credits);

                await ctx.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("The ProjectUsageNotifier was unable the update the DB or send the email, check the next log.");
                _logger.LogError(ex.Message, ex);
            }
        }

        private async Task<ProjectNotificationDetails?> GetProjectDetails(DatahubProjectDBContext ctx, int projectId, 
            CancellationToken cancellationToken)
        {
            var project = await ctx.Projects
                .Where(e => e.Project_ID == projectId)
                .Include(e => e.Credits)
                .Include(e => e.Users)
                .AsSingleQuery()
                .FirstOrDefaultAsync(cancellationToken);

            if (project is null)
                return default;

            var contacts = project.Users
                .Select(u => u.User_Name)
                .Where(IsValidEmail)
                .ToList();

            var budget = Convert.ToDouble(project.Project_Budget);

            return new(contacts, budget, project.Credits);
        }

        private EmailRequestMessage GetNotificationEmail(int perc, List<string> contacts)
        {
            var (subjectTemplate, bodyTemplate) = TemplateUtils.GetEmailTemplate("cost_alert.html");
            if (subjectTemplate is null || bodyTemplate is null)
                _logger.LogWarning("Email template file missing!");

            subjectTemplate ??= "{perc}% Datahub credits consumed";
            bodyTemplate ??= "<p>Your workspace has consumed {perc}% of the allocated credits in Datahub.</p>";

            var subject = subjectTemplate.Replace("{perc}", perc.ToString());
            var body = bodyTemplate.Replace("{perc}", perc.ToString());

            EmailRequestMessage notificationEmail = new() 
            { 
                To = contacts, 
                Subject = subject, 
                Body = body 
            };
            return notificationEmail;
        }

        private int GetNotificationPercent(int value)
        {
            return _notificationPercents.OrderByDescending(v => v).FirstOrDefault(v => value >= v);
        }

        static int[] ParseNotificationPercents(string percents)
        {
            return percents.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(s => int.Parse(s.Trim()))
                           .ToArray();
        }

        static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        static ProjectUsageNotificationMessage? DeserializeQueueMessage(string message)
        {
            return JsonSerializer.Deserialize<ProjectUsageNotificationMessage>(message);
        }

        record ProjectNotificationDetails(List<string> Contacts, double ProjectBudget, Project_Credits Credits);
    }
}
