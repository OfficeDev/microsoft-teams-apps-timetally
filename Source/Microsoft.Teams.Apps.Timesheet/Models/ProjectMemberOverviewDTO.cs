// <copyright file="ProjectMemberOverviewDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Holds the details of member entity to be shown on project utilization tab.
    /// </summary>
    public class ProjectMemberOverviewDTO
    {
        /// <summary>
        /// Gets or sets the project-user mapping unique Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user Id.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user is billable for a project.
        /// </summary>
        public bool IsBillable { get; set; }

        /// <summary>
        /// Gets or sets total hours of member.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "The hours must be greater than 0")]
        public int TotalHours { get; set; }

        /// <summary>
        /// Gets or sets name of member.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets isRemoved of member.
        /// </summary>
        public bool IsRemoved { get; set; }
    }
}
