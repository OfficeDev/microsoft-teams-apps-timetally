// <copyright file="ManagerReminderCard.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ReminderFunction.Services.AdaptiveCard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.Timesheet.Common.Resources;

    /// <summary>
    /// Creates card attachment for reminder notification to be sent to manager.
    /// </summary>
    public static class ManagerReminderCard
    {
        /// <summary>
        /// Get adaptive card attachment to notify manager regarding pending timesheet requests.
        /// </summary>
        /// <param name="localizer">String localizer for localizing user facing text.</param>
        /// <param name="applicationBasePath">Application base URL.</param>
        /// <param name="applicationManifestId">Manifest Id of application.</param>
        /// <param name="pendingRequestCount">Pending requests count for manager.</param>
        /// <returns>An adaptive card attachment.</returns>
        public static Attachment GetCard(IStringLocalizer<Strings> localizer, string applicationBasePath, string applicationManifestId, int pendingRequestCount)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri($"{applicationBasePath}/images/logo.png"),
                                        Size = AdaptiveImageSize.Medium,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Spacing = AdaptiveSpacing.None,
                                        Text = localizer.GetString("ManagerReminderCardTitle"),
                                        Wrap = true,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.None,
                                        Text = localizer.GetString("ManagerReminderCardSubTitle"),
                                        Wrap = true,
                                        IsSubtle = true,
                                    },
                                },
                                Width = AdaptiveColumnWidth.Stretch,
                            },
                        },
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("PendingRequests", pendingRequestCount.ToString(CultureInfo.InvariantCulture)),
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Url = new Uri($"https://teams.microsoft.com/l/entity/{applicationManifestId}/manager-dashboard"),
                        Title = $"{localizer.GetString("ManagerReminderCardButtonText")}",
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
