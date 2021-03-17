// <copyright file="SubmittedRequestDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// Represents the submitted timesheet.
    /// </summary>
    public class SubmittedRequestDTO
    {
        /// <summary>
        /// Gets or sets the user Id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the timesheet submitted date.
        /// </summary>
        public DateTime TimesheetDate { get; set; }

        /// <summary>
        /// Gets or sets the total hours.
        /// </summary>
        public int TotalHours { get; set; }

        /// <summary>
        /// Gets or sets the status of timesheet which belongs to <see cref="TimesheetStatus"/>.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets of sets submitted timesheet Ids.
        /// </summary>
        public IEnumerable<Guid> SubmittedTimesheetIds { get; set; }

        /// <summary>
        /// Gets or sets the project title of timesheet
        /// </summary>
#pragma warning disable CA2227
        public IEnumerable<string> ProjectTitles { get; set; }
#pragma warning restore CA2227
    }
}
