// <copyright file="ProjectTaskOverviewDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Holds the details of task entity to be shown on project utilization tab.
    /// </summary>
    public class ProjectTaskOverviewDTO
    {
        /// <summary>
        /// Gets or sets task Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets task title.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets total hours of member.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "The hours must be greater than 0")]
        public int TotalHours { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets isRemoved of task.
        /// </summary>
        public bool IsRemoved { get; set; }
    }
}
