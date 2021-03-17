// <copyright file="AdaptiveCardService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Services
{
    using System;
    using System.IO;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Cards;
    using Microsoft.Teams.Apps.Timesheet.Common.Resources;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Class that helps to return welcome card as attachment.
    /// </summary>
    public class AdaptiveCardService : IAdaptiveCardService
    {
        private const string WelcomeCardCacheKey = "_welcome-card";
        private const string RequestReminderCardCacheKey = "_requests-reminder-card";
        private const string ApprovedNotificationCardCacheKey = "_approved-notification-card";
        private const string RejectedNotificationCardCacheKey = "_rejected-notification-card";

        /// <summary>
        /// Memory cache instance to store and retrieve adaptive card payload.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Information about the web hosting environment an application is running in.
        /// </summary>
        private readonly IWebHostEnvironment env;

        /// <summary>
        /// The current cultures' string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// A set of key/value application configuration properties for Activity settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveCardService"/> class.
        /// </summary>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="env">Information about the web hosting environment an application is running in.</param>
        /// <param name="memoryCache">MemoryCache instance for caching authorization result.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for activity handler.</param>
        public AdaptiveCardService(IStringLocalizer<Strings> localizer, IMemoryCache memoryCache, IWebHostEnvironment env, IOptions<BotSettings> botOptions)
        {
            this.localizer = localizer;
            this.botOptions = botOptions;
            this.memoryCache = memoryCache;
            this.env = env;
        }

        /// <summary>
        /// Get pending requests reminder card for manager approval.
        /// </summary>
        /// <param name="requestsCount">Pending requests count.</param>
        /// <returns>Pending requests reminder card attachment.</returns>
        public Attachment GetRequestReminderCard(string requestsCount)
        {
            var cardPayload = this.GetCardPayload(RequestReminderCardCacheKey, "\\ManagerRequestsReminder\\request-reminder-card.json");
            var welcomeCardOptions = new RequestsReminderCard
            {
                AppImage = $"{this.botOptions.Value.AppBaseUri}/images/logo.png",
                DashboardTabUrl = $"https://teams.microsoft.com/l/entity/{this.botOptions.Value.ManifestId}/dashboard",
                ViewRequestButton = this.localizer.GetString("ViewRequestButton"),
                CardText = this.localizer.GetString("RequestsReminderCardText", requestsCount),
                CardSubtitle = this.localizer.GetString("ActionRequiredSubTitle"),
                CardTitle = this.localizer.GetString("TimesheetRequestsCardTitle"),
            };
            var template = new AdaptiveCardTemplate(cardPayload);
            var cardJson = template.Expand(welcomeCardOptions);
            AdaptiveCard card = AdaptiveCard.FromJson(cardJson).Card;

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Get timesheet approval notification card.
        /// </summary>
        /// <param name="cardDetails">Card details.</param>
        /// <returns>Timesheet approval notification card attachment.</returns>
        public Attachment GetApprovedNotificationCard(ApproveRejectCard cardDetails)
        {
            cardDetails = cardDetails ?? throw new ArgumentNullException(nameof(cardDetails));

            var cardPayload = this.GetCardPayload(ApprovedNotificationCardCacheKey, "\\TimesheetApprovedCard\\approved-card.json");
            var welcomeCardOptions = new ApproveRejectCard
            {
                TimesheetTabUrl = $"https://teams.microsoft.com/l/entity/{this.botOptions.Value.ManifestId}/timesheet",
                Date = cardDetails.Date,
                ProjectLabel = this.localizer.GetString("ProjectLabel"),
                ProjectTitle = cardDetails.ProjectTitle,
                HoursLabel = this.localizer.GetString("HoursLabel"),
                Hours = cardDetails.Hours,
                CardTitle = this.localizer.GetString("TimesheetApprovedCardTitle"),
                StatusLabel = this.localizer.GetString("ApprovedStatus"),
                ViewTimesheetButtonText = this.localizer.GetString("ViewTimesheetButtonText"),
            };
            var template = new AdaptiveCardTemplate(cardPayload);
            var cardJson = template.Expand(welcomeCardOptions);
            AdaptiveCard card = AdaptiveCard.FromJson(cardJson).Card;

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Get timesheet rejection notification card.
        /// </summary>
        /// <param name="cardDetails">Card details.</param>
        /// <returns>Timesheet rejection notification card attachment.</returns>
        public Attachment GetRejectedNotificationCard(ApproveRejectCard cardDetails)
        {
            cardDetails = cardDetails ?? throw new ArgumentNullException(nameof(cardDetails));

            var cardPayload = this.GetCardPayload(RejectedNotificationCardCacheKey, "\\TimesheetRejectedCard\\rejected-card.json");
            var welcomeCardOptions = new ApproveRejectCard
            {
                TimesheetTabUrl = $"https://teams.microsoft.com/l/entity/{this.botOptions.Value.ManifestId}/timesheet",
                Date = cardDetails.Date,
                ProjectLabel = this.localizer.GetString("ProjectLabel"),
                ProjectTitle = cardDetails.ProjectTitle,
                HoursLabel = this.localizer.GetString("HoursLabel"),
                Hours = cardDetails.Hours,
                CardTitle = this.localizer.GetString("TimesheetRejectedCardTitle"),
                CommentLabel = this.localizer.GetString("CommentLabel"),
                Comment = cardDetails.Comment,
                StatusLabel = this.localizer.GetString("RejectedStatus"),
                ViewTimesheetButtonText = this.localizer.GetString("ViewTimesheetButtonText"),
            };
            var template = new AdaptiveCardTemplate(cardPayload);
            var cardJson = template.Expand(welcomeCardOptions);
            AdaptiveCard card = AdaptiveCard.FromJson(cardJson).Card;

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Get welcome card attachment to be sent in personal scope.
        /// </summary>
        /// <returns>User welcome card attachment.</returns>
        public Attachment GetWelcomeCardForPersonalScope()
        {
            var cardPayload = this.GetCardPayload(WelcomeCardCacheKey, "\\WelcomeCard\\welcome-card.json");
            var welcomeCardOptions = new WelcomeCard
            {
                AppImage = $"{this.botOptions.Value.AppBaseUri}/images/logo.png",
                TimesheetTabUrl = $"https://teams.microsoft.com/l/entity/{this.botOptions.Value.ManifestId}/timesheet",
                WelcomeCardFillTimesheetButton = this.localizer.GetString("FillTimesheetButton"),
                WelcomeCardIntro = this.localizer.GetString("WelcomeCardIntro"),
                WelcomeCardSubtitle = this.localizer.GetString("WelcomeCardSubtitle"),
                WelcomeCardTitle = this.localizer.GetString("WelcomeCardTitle"),
            };
            var template = new AdaptiveCardTemplate(cardPayload);
            var cardJson = template.Expand(welcomeCardOptions);
            AdaptiveCard card = AdaptiveCard.FromJson(cardJson).Card;

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Get card payload from memory.
        /// </summary>
        /// <param name="cardCacheKey">Card cache key.</param>
        /// <param name="jsonTemplateFileName">File name for JSON adaptive card template.</param>
        /// <returns>Returns adaptive card payload in JSON format.</returns>
        private string GetCardPayload(string cardCacheKey, string jsonTemplateFileName)
        {
            bool isCacheEntryExists = this.memoryCache.TryGetValue(cardCacheKey, out string cardPayload);

            if (!isCacheEntryExists)
            {
                // If cache duration is not specified then by default cache for 12 hours.
                var cacheDurationInHour = TimeSpan.FromHours(this.botOptions.Value.CardCacheDurationInHour);
                cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(12) : cacheDurationInHour;

                var cardJsonFilePath = Path.Combine(this.env.ContentRootPath, $".\\Cards\\{jsonTemplateFileName}");
                cardPayload = File.ReadAllText(cardJsonFilePath);
                this.memoryCache.Set(cardCacheKey, cardPayload, cacheDurationInHour);
            }

            return cardPayload;
        }
    }
}