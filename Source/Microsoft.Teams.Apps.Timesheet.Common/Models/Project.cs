// <copyright file="Project.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds the details of a project entity.
    /// </summary>
    public partial class Project
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        public Project()
        {
            this.Tasks = new HashSet<TaskEntity>();
            this.Members = new HashSet<Member>();
        }

        /// <summary>
        /// Gets or sets project Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets project title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets client name.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets billable hours.
        /// </summary>
        public int BillableHours { get; set; }

        /// <summary>
        /// Gets or sets non-billable hours.
        /// </summary>
        public int NonBillableHours { get; set; }

        /// <summary>
        /// Gets or sets date when project starts.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets date when project ends.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets user object Id who created the project.
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets date when the project was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets task details.
        /// </summary>
#pragma warning disable CA2227 // Need to add/remove tasks while creating/updating a project
        public virtual ICollection<TaskEntity> Tasks { get; set; }
#pragma warning restore CA2227 // Need to add/remove tasks while creating/updating a project

        /// <summary>
        /// Gets or sets the project members.
        /// </summary>
#pragma warning disable CA2227 // Need to add/remove members while creating/updating a project
        public virtual ICollection<Member> Members { get; set; }
#pragma warning restore CA2227 // Need to add/remove members while creating/updating a project
    }
}
