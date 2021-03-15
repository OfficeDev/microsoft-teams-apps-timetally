// <copyright file="FunctionOptions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ReminderFunction
{
    /// <summary>
    /// Options used in SendReminder azure function.
    /// </summary>
    public class FunctionOptions
    {
        /// <summary>
        /// Gets or sets the manifest Id of application.
        /// </summary>
        public string ManifestId { get; set; }

        /// <summary>
        /// Gets or sets the application base URL.
        /// </summary>
        public string AppBaseUri { get; set; }
    }
}
