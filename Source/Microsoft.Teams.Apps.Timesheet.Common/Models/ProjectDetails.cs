// <copyright file="ProjectDetails.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the projects details and related tasks.
    /// </summary>
    public class ProjectDetails
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
        /// Gets or sets project start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets project end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the timesheet details.
        /// </summary>
#pragma warning disable CA2227 // Need to add timesheet details in list.
        [Required]
        public List<TimesheetDetails> TimesheetDetails { get; set; }
#pragma warning restore CA2227 // Need to add timesheet details in list.
    }
}
