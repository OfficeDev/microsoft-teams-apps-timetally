// <copyright file="DashboardRequestDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// Represents the timesheet requests which are submitted for manager's approval.
    /// </summary>
    public class DashboardRequestDTO
    {
        /// <summary>
        /// Gets or sets the user Id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the number of days request submitted for.
        /// </summary>
        public int NumberOfDays { get; set; }

        /// <summary>
        /// Gets or sets the total hours.
        /// </summary>
        public int TotalHours { get; set; }

        /// <summary>
        /// Gets or sets the status of dashboard which belongs to <see cref="TimesheetStatus"/>.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the list of submitted timesheet Ids by reportee.
        /// </summary>
        public IEnumerable<Guid> SubmittedTimesheetIds { get; set; }

        /// <summary>
        /// Gets or sets the timesheet dates of dashboard request.
        /// </summary>
#pragma warning disable CA2227
        public List<List<DateTime>> RequestedForDates { get; set; }
#pragma warning restore CA2227
    }
}
