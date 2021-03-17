// <copyright file="UserTimesheet.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the timesheet of a particular user for particular date.
    /// </summary>
    public class UserTimesheet
    {
        private DateTime timesheetDate;

        /// <summary>
        /// Gets or sets the calendar date.
        /// </summary>
        public DateTime TimesheetDate
        {
            get { return this.timesheetDate.Date; }
            set { this.timesheetDate = value.Date; }
        }

        /// <summary>
        /// Gets or sets the project details.
        /// </summary>
#pragma warning disable CA2227 // Need to add values in list
        [Required]
        public List<ProjectDetails> ProjectDetails { get; set; }
#pragma warning restore CA2227 // Need to add values in list
    }
}
