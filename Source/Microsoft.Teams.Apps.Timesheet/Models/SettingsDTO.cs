// <copyright file="SettingsDTO.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    /// <summary>
    /// Describes the application settings information.
    /// </summary>
    public class SettingsDTO
    {
        /// <summary>
        /// Gets or sets timesheet freezing day of month.
        /// </summary>
        public int TimesheetFreezeDayOfMonth { get; set; }

        /// <summary>
        /// Gets or sets maximum hours can be filled in a week.
        /// </summary>
        public int WeeklyEffortsLimit { get; set; }
    }
}