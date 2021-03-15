// <copyright file="TimesheetEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;

    /// <summary>
    /// Holds the information of a timesheet entity.
    /// </summary>
    public partial class TimesheetEntity
    {
        /// <summary>
        /// Gets or sets the Id of timesheet entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the task Id.
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// Gets or sets the task title.
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets or sets the user Id to whom the task was assigned.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets calendar date for which efforts invested.
        /// </summary>
        public DateTime TimesheetDate { get; set; }

        /// <summary>
        /// Gets or sets utilized efforts.
        /// </summary>
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
        /// Gets or sets date when the task efforts submitted to manager.
        /// </summary>
        public DateTime? SubmittedOn { get; set; }

        /// <summary>
        /// Gets or sets date when the task was lastly modified.
        /// </summary>
        public DateTime? LastModifiedOn { get; set; }

        /// <summary>
        /// Gets or sets the task details.
        /// </summary>
        public virtual TaskEntity Task { get; set; }
    }
}