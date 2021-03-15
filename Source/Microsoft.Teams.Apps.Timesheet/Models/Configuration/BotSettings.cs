// <copyright file="BotSettings.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using Microsoft.Teams.Apps.Timesheet.Common.Services.CommonBot;

    /// <summary>
    /// A class which helps to provide Bot settings for application.
    /// </summary>
    public class BotSettings : BotOptions
    {
        /// <summary>
        /// Gets or sets application base Uri which helps in generating customer token.
        /// </summary>
        public string AppBaseUri { get; set; }

        /// <summary>
        /// Gets or sets application manifest id.
        /// </summary>
        public string ManifestId { get; set; }

        /// <summary>
        /// Gets or sets cache duration for card payload.
        /// </summary>
        public int CardCacheDurationInHour { get; set; }

        /// <summary>
        /// Gets or sets cache duration for policy indicating whether a logged-in user is part of any projects.
        /// </summary>
        public int UserPartOfProjectsCacheDurationInHour { get; set; }

        /// <summary>
        /// Gets or sets timesheet freeze day of month after which previous month timesheet will get frozen.
        /// </summary>
        public int TimesheetFreezeDayOfMonth { get; set; }

        /// <summary>
        /// Gets or sets efforts limit that can be filled per week.
        /// </summary>
        public int WeeklyEffortsLimit { get; set; }

        /// <summary>
        /// Gets or sets daily efforts limit that can be filled per day.
        /// </summary>
        public int DailyEffortsLimit { get; set; }

        /// <summary>
        /// Gets or sets cache duration in hours for storing reportees of manager.
        /// </summary>
        public int ManagerReporteesCacheDurationInHours { get; set; }

        /// <summary>
        /// Gets or sets cache duration for caching validation result for checking whether manager has created the requested project.
        /// </summary>
        public int ManagerProjectValidationCacheDurationInHours { get; set; }
    }
}