// <copyright file="FillTimesheetReminderCard.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ReminderFunction.Services.AdaptiveCard
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.Timesheet.Common.Resources;

    /// <summary>
    /// Creates card attachment for timesheet reminder notification to be sent to project members.
    /// </summary>
    public static class FillTimesheetReminderCard
    {
        /// <summary>
        /// Get adaptive card attachment to notify project members for filling timesheet.
        /// </summary>
        /// <param name="localizer">String localizer for localizing user facing text.</param>
        /// <param name="applicationManifestId">Manifest Id of application.</param>
        /// <returns>An adaptive card attachment.</returns>
        public static Attachment GetCard(IStringLocalizer<Strings> localizer, string applicationManifestId)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large,
                        Spacing = AdaptiveSpacing.None,
                        Text = localizer.GetString("FillTimesheetReminderCardTitle"),
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Url = new Uri($"https://teams.microsoft.com/l/entity/{applicationManifestId}/timesheet"),
                        Title = $"{localizer.GetString("FillTimesheetButton")}",
                    },
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}
