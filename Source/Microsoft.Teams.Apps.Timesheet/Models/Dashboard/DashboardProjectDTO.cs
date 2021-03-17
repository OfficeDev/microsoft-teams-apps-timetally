// <copyright file="DashboardProjectDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;

    /// <summary>
    /// Holds the details of a project entity to show on dashboard.
    /// </summary>
    public class DashboardProjectDTO
    {
        /// <summary>
        /// Gets or sets project Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets project title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets monthly total hours of project.
        /// </summary>
        public int TotalHours { get; set; }

        /// <summary>
        /// Gets or sets monthly utilize hours of project.
        /// </summary>
        public int UtilizedHours { get; set; }
    }
}
