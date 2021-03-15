// <copyright file="Member.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;

    /// <summary>
    /// The class represents user-project mapping entity.
    /// </summary>
    public partial class Member
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
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user is billable for a project.
        /// </summary>
        public bool IsBillable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a user was removed from project.
        /// </summary>
        public bool IsRemoved { get; set; }

        /// <summary>
        /// Gets or sets the project details.
        /// </summary>
        public virtual Project Project { get; set; }
    }
}
