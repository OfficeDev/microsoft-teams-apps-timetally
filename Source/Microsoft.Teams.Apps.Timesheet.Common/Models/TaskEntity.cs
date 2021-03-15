// <copyright file="TaskEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds the details of a task entity.
    /// </summary>
    public partial class TaskEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskEntity"/> class.
        /// </summary>
        public TaskEntity()
        {
            this.Timesheets = new HashSet<TimesheetEntity>();
        }

        /// <summary>
        /// Gets or sets task Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets task title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a task was deleted.
        /// </summary>
        public bool IsRemoved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a task added by member.
        /// </summary>
        public bool IsAddedByMember { get; set; }

        /// <summary>
        /// Gets or sets member mapping Id.
        /// </summary>
        public Guid? MemberMappingId { get; set; }

        /// <summary>
        /// Gets or sets start date of a task.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets end date of a task.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets project Id.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Gets or sets project details.
        /// </summary>
        public virtual Project Project { get; set; }

        /// <summary>
        /// Gets or sets timesheet details.
        /// </summary>
#pragma warning disable CA2227 // Need to add/remove timesheet details for a task
        public virtual ICollection<TimesheetEntity> Timesheets { get; set; }
#pragma warning restore CA2227 // Need to add/remove timesheet details for a task

        /// <summary>
        /// Gets or sets member details.
        /// </summary>
        public virtual Member MemberMapping { get; set; }
    }
}