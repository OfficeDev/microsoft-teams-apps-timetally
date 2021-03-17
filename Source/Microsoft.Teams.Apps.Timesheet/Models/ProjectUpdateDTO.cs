// <copyright file="ProjectUpdateDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Holds the details of a project entity to be updated.
    /// </summary>
    public class ProjectUpdateDTO
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
    }
}