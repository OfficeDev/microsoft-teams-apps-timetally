// <copyright file="ApproveRejectCard.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Cards
{
    /// <summary>
    /// A class to hold welcome card information.
    /// </summary>
    public class ApproveRejectCard
    {
        /// <summary>
        /// Gets or sets card title.
        /// </summary>
        public string CardTitle { get; set; }

        /// <summary>
        /// Gets or sets date string of timesheet request.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets localized status label for timesheet.
        /// </summary>
        public string StatusLabel { get; set; }

        /// <summary>
        /// Gets or sets localized project label.
        /// </summary>
        public string ProjectLabel { get; set; }

        /// <summary>
        /// Gets or sets project title for timesheet.
        /// </summary>
        public string ProjectTitle { get; set; }

        /// <summary>
        /// Gets or sets localized hours label.
        /// </summary>
        public string HoursLabel { get; set; }

        /// <summary>
        /// Gets or sets total hours filled in for the day.
        /// </summary>
        public string Hours { get; set; }

        /// <summary>
        /// Gets or sets localized comment label.
        /// </summary>
        public string CommentLabel { get; set; }

        /// <summary>
        /// Gets or sets manager comment for timesheet.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets button label for view timesheet.
        /// </summary>
        public string ViewTimesheetButtonText { get; set; }

        /// <summary>
        /// Gets or sets URL to timesheet tab.
        /// </summary>
        public string TimesheetTabUrl { get; set; }
    }
}