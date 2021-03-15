// <copyright file="RequestsReminderCard.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Cards
{
    /// <summary>
    /// A class to hold reminder information for manager.
    /// </summary>
    public class RequestsReminderCard
    {
        /// <summary>
        /// Gets or sets application logo URL.
        /// </summary>
        public string AppImage { get; set; }

        /// <summary>
        /// Gets or sets card title.
        /// </summary>
        public string CardTitle { get; set; }

        /// <summary>
        /// Gets or sets card sub-title.
        /// </summary>
        public string CardSubtitle { get; set; }

        /// <summary>
        /// Gets or sets card body text.
        /// </summary>
        public string CardText { get; set; }

        /// <summary>
        /// Gets or sets button text for view requests button.
        /// </summary>
        public string ViewRequestButton { get; set; }

        /// <summary>
        /// Gets or sets URL to dashboard tab.
        /// </summary>
        public string DashboardTabUrl { get; set; }
    }
}