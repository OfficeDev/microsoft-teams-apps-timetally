// <copyright file="RepositoryOptions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    /// <summary>
    /// Options used for creating repositories.
    /// </summary>
    public class RepositoryOptions
    {
        /// <summary>
        /// Gets or sets the storage account connection string.
        /// </summary>
        public string StorageAccountConnectionString { get; set; }
    }
}
