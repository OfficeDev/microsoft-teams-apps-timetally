// <copyright file="SendReminderFunction.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ReminderFunction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Common.Resources;
    using Microsoft.Teams.Apps.Timesheet.Common.Services.Message;
    using Microsoft.Teams.Apps.Timesheet.ReminderFunction.Services.AdaptiveCard;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Azure function to send reminders to manager and project members.
    /// </summary>
    public class SendReminderFunction
    {
        private readonly IMessageService messageService;
        private readonly IRepositoryAccessors repositoryAccessors;
        private readonly IStringLocalizer<Strings> localizer;
        private readonly string manifestId;
        private readonly string appBaseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendReminderFunction"/> class.
        /// </summary>
        /// <param name="messageService">Instance of message service for sending notifications.</param>
        /// <param name="repositoryAccessors">Instance of repository accessor for fetching information from database.</param>
        /// <param name="options">Send reminder function options.</param>
        /// <param name="localizer">Instance of localizer.</param>
        public SendReminderFunction(IMessageService messageService, IRepositoryAccessors repositoryAccessors, IOptions<FunctionOptions> options, IStringLocalizer<Strings> localizer)
        {
            this.messageService = messageService;
            this.repositoryAccessors = repositoryAccessors;
            this.manifestId = options?.Value?.ManifestId;
            this.appBaseUrl = options?.Value?.AppBaseUri;
            this.localizer = localizer;
        }

        /// <summary>
        /// Time triggered function to send notifications at configured CRON expression value in application settings.
        /// </summary>
        /// <param name="timer">Timer info instance.</param>
        /// <param name="logger">Instance of logger to log errors and information.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("SendReminderFunction")]
        public async Task RunAsync([TimerTrigger("%TimerInterval%")] TimerInfo timer, ILogger logger)
        {
            timer = timer ?? throw new ArgumentNullException(nameof(timer));
            logger.LogInformation("SendReminderFunction function started.");
            await this.SendPendingRequestsRemindersAsync(logger);
            await this.SendFillTimesheetRemindersAsync(logger);
            logger.LogInformation("SendReminderFunction function finished, next occurrence at: " + timer.FormatNextOccurrences(1));
        }

        /// <summary>
        /// Sends reminder to manager for approval of pending timesheet requests.
        /// </summary>
        /// <param name="logger">Instance of logger to log errors and information.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task SendPendingRequestsRemindersAsync(ILogger logger)
        {
            var managerUserIds = this.repositoryAccessors.ProjectRepository.GetAllManagersUserIDs();

            foreach (var managerId in managerUserIds)
            {
                var userConversations = await this.repositoryAccessors.ConversationRepository.FindAsync(conversation => conversation.UserId == managerId);
                var conversatinDetails = userConversations.FirstOrDefault();
                if (conversatinDetails != null)
                {
                    var pendingRequests = this.repositoryAccessors.TimesheetRepository.GetTimesheetsByManagerId(managerId, TimesheetStatus.Submitted);

                    if (pendingRequests.Count > 0)
                    {
                        var card = ManagerReminderCard.GetCard(this.localizer, this.appBaseUrl, this.manifestId, pendingRequests.Count);
                        await this.messageService.SendMessageAsync(MessageFactory.Attachment(card), conversatinDetails.ConversationId, new Uri(conversatinDetails.ServiceUrl), 2, logger);
                    }
                }
            }
        }

        /// <summary>
        /// Sends reminder to project members for filling timesheet for current date.
        /// </summary>
        /// <param name="logger">Instance of logger to log errors and information.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task SendFillTimesheetRemindersAsync(ILogger logger)
        {
            var currentDate = DateTime.Now.Date;
            var projects = await this.repositoryAccessors.ProjectRepository.FindAsync(project => ((project.StartDate.Date >= currentDate && project.StartDate.Date <= currentDate.AddDays(1)) ||
                (project.StartDate.Date < currentDate && project.EndDate.Date >= currentDate)));

            Dictionary<Guid, Conversation> usersEligibleForNotification = new Dictionary<Guid, Conversation>();

            foreach (var project in projects)
            {
                var members = await this.repositoryAccessors.MemberRepository.FindAsync(member => member.ProjectId == project.Id);
                var memberUserIds = members.Select(member => member.UserId);
                var conversations = await this.repositoryAccessors.ConversationRepository.FindAsync(conversation => memberUserIds.Contains(conversation.UserId));

                foreach (var userConversation in conversations)
                {
                    if (!usersEligibleForNotification.ContainsKey(userConversation.UserId))
                    {
                        usersEligibleForNotification.Add(userConversation.UserId, userConversation);
                    }
                }
            }

            var card = FillTimesheetReminderCard.GetCard(this.localizer, this.manifestId);

            foreach (var userConversation in usersEligibleForNotification)
            {
                await this.messageService.SendMessageAsync(MessageFactory.Attachment(card), userConversation.Value.ConversationId, new Uri(userConversation.Value.ServiceUrl), 2, logger);
            }
        }
    }
}
