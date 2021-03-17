// <copyright file="AppLifecycleHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Bot
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Services;

    /// <summary>
    /// Helper for handling bot related activities.
    /// </summary>
    public class AppLifecycleHandler : IAppLifecycleHandler
    {
        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<AppLifecycleHandler> logger;

        /// <summary>
        /// Instance of adaptive card service to create and get adaptive cards.
        /// </summary>
        private readonly IAdaptiveCardService adaptiveCardService;

        /// <summary>
        /// The instance of repository accessors to access repositories.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLifecycleHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="adaptiveCardService">Instance of adaptive card service to create and get adaptive cards.</param>
        /// <param name="repositoryAccessors">The instance of repository accessors.</param>
        public AppLifecycleHandler(
            ILogger<AppLifecycleHandler> logger,
            IAdaptiveCardService adaptiveCardService,
            IRepositoryAccessors repositoryAccessors)
        {
            this.logger = logger;
            this.adaptiveCardService = adaptiveCardService;
            this.repositoryAccessors = repositoryAccessors;
        }

        /// <summary>
        /// Sends welcome card to user when bot is installed in personal scope.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        public async Task OnBotInstalledInPersonalAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext), "Turncontext cannot be null");

            this.logger.LogInformation($"Bot added in personal scope for user {turnContext.Activity.From.AadObjectId}");

            var activity = turnContext.Activity;

            // Add or update user details when bot is installed.
            var existingRecord = await this.repositoryAccessors.ConversationRepository.GetConversationsByUserIdAsync(Guid.Parse(turnContext.Activity.From.AadObjectId));

            if (existingRecord.Any())
            {
                var userConversation = existingRecord.First();
                userConversation.ConversationId = activity.Conversation.Id;
                userConversation.ServiceUrl = activity.ServiceUrl;
                userConversation.BotInstalledOn = DateTime.UtcNow;

                this.repositoryAccessors.ConversationRepository.Update(userConversation);
            }
            else
            {
                var userWelcomeCardAttachment = this.adaptiveCardService.GetWelcomeCardForPersonalScope();
                await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment));

                var userConversationDetails = new Conversation
                {
                    BotInstalledOn = DateTime.Now,
                    ConversationId = activity.Conversation.Id,
                    ServiceUrl = activity.ServiceUrl,
                    UserId = Guid.Parse(turnContext.Activity.From.AadObjectId),
                };

                this.repositoryAccessors.ConversationRepository.Add(userConversationDetails);
            }

            await this.repositoryAccessors.SaveChangesAsync();
            this.logger.LogInformation($"Successfully installed app for user {activity.From.AadObjectId}.");
        }
    }
}
