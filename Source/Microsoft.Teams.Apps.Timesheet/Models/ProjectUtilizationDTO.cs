// <copyright file="ProjectUtilizationDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;

    /// <summary>
    /// Holds the details of a project utilization entity.
    /// </summary>
    public class ProjectUtilizationDTO
    {
        /// <summary>
        /// Gets or sets Id of project.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets title of project.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets utilized billable hours of project.
        /// </summary>
        public int BillableUtilizedHours { get; set; }

        /// <summary>
        /// Gets or sets  utilized non-billable hours of project.
        /// </summary>
        public int NonBillableUtilizedHours { get; set; }

        /// <summary>
        /// Gets or sets underutilized billable hours of project.
        /// </summary>
        public int BillableUnderutilizedHours { get; set; }

        /// <summary>
        /// Gets or sets underutilized non-billable hours of project.
        /// </summary>
        public int NonBillableUnderutilizedHours { get; set; }

        /// <summary>
        /// Gets or sets start date of the project.
        /// </summary>
        public DateTime ProjectStartDate { get; set; }

        /// <summary>
        /// Gets or sets end date of the project.
        /// </summary>
        public DateTime ProjectEndDate { get; set; }

        /// <summary>
        /// Gets or sets total hours of project.
        /// </summary>
        public int TotalHours { get; set; }
    }
}