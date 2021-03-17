// <copyright file="IAdaptiveCardService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Services
{
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Timesheet.Cards;

    /// <summary>
    /// Provides methods for creating adaptive cards.
    /// </summary>
    public interface IAdaptiveCardService
    {
        /// <summary>
        /// Get welcome card attachment to be sent in personal scope.
        /// </summary>
        /// <returns>User welcome card attachment.</returns>
        Attachment GetWelcomeCardForPersonalScope();

        /// <summary>
        /// Get pending requests reminder card for manager approval.
        /// </summary>
        /// <param name="requestsCount">Pending requests count.</param>
        /// <returns>Pending requests reminder card attachment.</returns>
        Attachment GetRequestReminderCard(string requestsCount);

        /// <summary>
        /// Get timesheet approval notification card.
        /// </summary>
        /// <param name="cardDetails">Card details.</param>
        /// <returns>Timesheet approval notification card attachment.</returns>
        Attachment GetApprovedNotificationCard(ApproveRejectCard cardDetails);

        /// <summary>
        /// Get timesheet rejection notification card.
        /// </summary>
        /// <param name="cardDetails">Card details.</param>
        /// <returns>Timesheet rejection notification card attachment.</returns>
        Attachment GetRejectedNotificationCard(ApproveRejectCard cardDetails);
    }
}