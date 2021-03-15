// <copyright file="TimesheetDetails.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the timesheet details.
    /// </summary>
    public class TimesheetDetails
    {
        /// <summary>
        /// Gets or sets task Id.
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// Gets or sets task title.
        /// </summary>
        [Required]
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets or sets utilized efforts.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "The efforts must be a valid number.")]
        public int Hours { get; set; }

        /// <summary>
        /// Gets or sets the status of current task which belongs to <see cref="TimesheetStatus"/>
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets manager comments.
        /// </summary>
        public string ManagerComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a task added my project member.
        /// </summary>
        public bool IsAddedByMember { get; set; }

        /// <summary>
        /// Gets or sets task start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets task end date.
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
