// <copyright file="ITimesheetHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Provides helper methods for managing timesheet.
    /// </summary>
    public interface ITimesheetHelper
    {
        /// <summary>
        /// Duplicates the efforts of source date timesheet to the target dates.
        /// </summary>
        /// <param name="sourceDate">The source date of which efforts needs to be duplicated.</param>
        /// <param name="targetDates">The target dates to which efforts needs to be duplicated.</param>
        /// <param name="clientLocalCurrentDate">The client's local current date.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns duplicated timesheets.</returns>
        Task<List<TimesheetDTO>> DuplicateEffortsAsync(DateTime sourceDate, IEnumerable<DateTime> targetDates, DateTime clientLocalCurrentDate, Guid userObjectId);

        /// <summary>
        /// Creates a new timesheet entry for a date if not exists or updates the existing one for provided dates
        /// with status as "Saved".
        /// </summary>
        /// <param name="userTimesheets">The timesheet details that need to be saved.</param>
        /// <param name="clientLocalCurrentDate">The client's local current date.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Saved timesheet entries.</returns>
        Task<List<TimesheetDTO>> SaveTimesheetsAsync(IEnumerable<UserTimesheet> userTimesheets, DateTime clientLocalCurrentDate, Guid userObjectId);

        /// <summary>
        /// Updates the status of all saved timesheets to submitted.
        /// </summary>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns true if timesheets submitted successfully. Else returns false.</returns>
        Task<List<TimesheetDTO>> SubmitTimesheetsAsync(Guid userObjectId);

        /// <summary>
        /// Gets timesheets of user between specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="userObjectId">The user Id of which timesheets to get.</param>
        /// <returns>Returns timesheets of user for particular date range.</returns>
        Task<IEnumerable<UserTimesheet>> GetTimesheetsAsync(DateTime calendarStartDate, DateTime calendarEndDate, Guid userObjectId);

        /// <summary>
        /// Checks whether client current date is valid.
        /// </summary>
        /// <param name="clientCurrentDate">The client's local current date.</param>
        /// <param name="utcDate">The current UTC date.</param>
        /// <returns>Returns true if the current date is valid. Else returns false.</returns>
        public bool IsClientCurrentDateValid(DateTime clientCurrentDate, DateTime utcDate);

        /// <summary>
        /// Gets the active timesheet.
        /// </summary>
        /// <param name="reporteeObjectId">The user Id of which timesheets to get.</param>
        /// <param name="status">Timesheet status for filtering.</param>
        /// <returns>Returns the list of timesheets.</returns>
        IEnumerable<SubmittedRequestDTO> GetTimesheetsByStatus(Guid reporteeObjectId, TimesheetStatus status);

        /// <summary>
        /// To approve or reject the timesheets.
        /// </summary>
        /// <param name="timesheets">Timesheets to be approved or rejected.
        /// Timesheets are validated at controller that it should be submitted to the logged-in manager.</param>
        /// <param name="timesheetApprovals">The details of timesheets which are approved or reject by the manager.</param>
        /// <param name="status">If true, the timesheet get approved. Else timesheet get rejected.</param>
        /// <returns>Returns true if timesheets approved or rejected successfully. Else returns false.</returns>
        Task<bool> ApproveOrRejectTimesheetsAsync(IEnumerable<TimesheetEntity> timesheets, IEnumerable<RequestApprovalDTO> timesheetApprovals, TimesheetStatus status);

        /// <summary>
        /// Gets submitted timesheets by Ids.
        /// </summary>
        /// <param name="managerObjectId">Manager object Id who has created the project.</param>
        /// <param name="timesheetIds">Ids of timesheet to fetch.</param>
        /// <returns>Return timesheet if all timesheet found, else return null.</returns>
        IEnumerable<TimesheetEntity> GetSubmittedTimesheetsByIds(Guid managerObjectId, IEnumerable<Guid> timesheetIds);

        /// <summary>
        /// Gets timesheet dates those aren't frozen.
        /// </summary>
        /// <param name="timesheetDates">The timesheet dates that need to be filtered.</param>
        /// <param name="clientLocalCurrentDate">The client's local current date.</param>
        /// <returns>Returns true if a timesheet date is frozen. Else return false.</returns>
        IEnumerable<DateTime> GetNotYetFrozenTimesheetDates(IEnumerable<DateTime> timesheetDates, DateTimeOffset clientLocalCurrentDate);
    }
}
