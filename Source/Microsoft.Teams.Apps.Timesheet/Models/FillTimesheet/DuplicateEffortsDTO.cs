// <copyright file="DuplicateEffortsDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents duplicate efforts view model.
    /// </summary>
    public class DuplicateEffortsDTO
    {
        /// <summary>
        /// Gets or sets source date of which efforts needs to be duplicated.
        /// </summary>
        public DateTime SourceDate { get; set; }

        /// <summary>
        /// Gets or sets the target dates to which efforts needs to be duplicated.
        /// </summary>
        [Required]
        public IEnumerable<DateTime> TargetDates { get; set; }
    }
}
