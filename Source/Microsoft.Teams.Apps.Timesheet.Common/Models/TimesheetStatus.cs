// <copyright file="TimesheetStatus.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    /// <summary>
    /// The enumeration that holds the status of timesheet.
    /// </summary>
    public enum TimesheetStatus
    {
        /// <summary>
        /// Represents nothing has been done in timesheet.
        /// </summary>
        None,

        /// <summary>
        /// Indicated that the timesheet saved by user.
        /// </summary>
        Saved,

        /// <summary>
        /// Represents that the timesheet submitted by user.
        /// </summary>
        Submitted,

        /// <summary>
        /// Represents that the timesheet approved by manager.
        /// </summary>
        Approved,

        /// <summary>
        /// Represents that the timesheet rejected by manager.
        /// </summary>
        Rejected,
    }
}
