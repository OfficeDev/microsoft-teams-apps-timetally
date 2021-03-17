// <copyright file="GraphPermissionType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Authentication
{
    /// <summary>
    /// Microsoft Graph permission types.
    /// </summary>
    public enum GraphPermissionType
    {
        /// <summary>
        /// This represents application permission of Microsoft Graph.
        /// </summary>
        Application,

        /// <summary>
        /// This represents delegate permission of Microsoft Graph.
        /// </summary>
        UserDelegated,
    }
}
