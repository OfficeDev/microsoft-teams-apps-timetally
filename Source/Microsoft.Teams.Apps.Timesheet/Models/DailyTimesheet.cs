// <copyright file="DailyTimesheet.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;

    /// <summary>
    /// Represents user's efforts details for particular calendar date.
    /// </summary>
    public class DailyTimesheet
    {
        /// <summary>
        /// Gets or sets project id.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Gets or sets project title.
        /// </summary>
        public string ProjectTitle { get; set; }

        /// <summary>
        /// Gets or sets task id.
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// Gets or sets task title.
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets or sets calendar date for which efforts invested.
        /// </summary>
        public DateTime TimesheetDate { get; set; }

        /// <summary>
        /// Gets or sets utilized efforts.
        /// </summary>
        public short Hours { get; set; }
    }
}
