// <copyright file="MemberDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The class represents user-project mapping entity.
    /// </summary>
    public partial class MemberDTO
    {
        /// <summary>
        /// Gets or sets the project-user mapping unique Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the project Id.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the user Id.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user is billable for a project.
        /// </summary>
        public bool IsBillable { get; set; }
    }
}
