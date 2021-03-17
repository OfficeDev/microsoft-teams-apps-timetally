// <copyright file="UserDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    /// <summary>
    /// Holds the details of a user.
    /// </summary>
    public class UserDTO
    {
        /// <summary>
        /// Gets or sets user Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets user's display name.
        /// </summary>
        public string DisplayName { get; set; }
    }
}