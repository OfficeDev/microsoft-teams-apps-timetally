// <copyright file="ProjectDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Teams.Apps.GoodReads.Helpers.CustomValidations;

    /// <summary>
    /// Holds the details of a project entity.
    /// </summary>
    public class ProjectDTO
    {
        /// <summary>
        /// Gets or sets project Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets project title.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets client name.
        /// </summary>
        [MaxLength(50)]
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets billable hours.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int BillableHours { get; set; }

        /// <summary>
        /// Gets or sets non-billable hours.
        /// </summary>
        [Range(0, int.MaxValue)]
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
        /// Gets or sets task details.
        /// </summary>
#pragma warning disable CA2227 // Need to add/remove tasks while creating/updating a project
        [TasksValidation(5, 300)]
        public List<TaskDTO> Tasks { get; set; }
#pragma warning restore CA2227 // Need to add/remove tasks while creating/updating a project

        /// <summary>
        /// Gets or sets the project members.
        /// </summary>
#pragma warning disable CA2227 // Need to add/remove members while creating/updating a project
        public List<MemberDTO> Members { get; set; }
#pragma warning restore CA2227 // Need to add/remove members while creating/updating a project
    }
}