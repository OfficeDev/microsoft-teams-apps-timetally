// <copyright file="WelcomeCard.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Cards
{
    /// <summary>
    /// A class to hold welcome card information.
    /// </summary>
    public class WelcomeCard
    {
        /// <summary>
        /// Gets or sets application logo URL.
        /// </summary>
        public string AppImage { get; set; }

        /// <summary>
        /// Gets or sets welcome card title.
        /// </summary>
        public string WelcomeCardTitle { get; set; }

        /// <summary>
        /// Gets or sets welcome card sub-title.
        /// </summary>
        public string WelcomeCardSubtitle { get; set; }

        /// <summary>
        /// Gets or sets welcome card introduction.
        /// </summary>
        public string WelcomeCardIntro { get; set; }

        /// <summary>
        /// Gets or sets button text for fill timesheet button.
        /// </summary>
        public string WelcomeCardFillTimesheetButton { get; set; }

        /// <summary>
        /// Gets or sets URL to timesheet tab.
        /// </summary>
        public string TimesheetTabUrl { get; set; }
    }
}