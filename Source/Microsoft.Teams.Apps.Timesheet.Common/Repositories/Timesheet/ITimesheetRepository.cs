// <copyright file="ITimesheetRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// Exposes methods which will be used to perform database operations on timesheet entity.
    /// </summary>
    public interface ITimesheetRepository : IBaseRepository<TimesheetEntity>
    {
        /// <summary>
        /// Updates timesheet entries.
        /// </summary>
        /// <param name="timesheets">The list of timesheet entries to be updated.</param>
        void Update(IEnumerable<TimesheetEntity> timesheets);

        /// <summary>
        /// Gets the timesheet requests.
        /// </summary>
        /// <param name="userId">The user Id of which requests to get.</param>
        /// <param name="timesheetDates">The dates of requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        Task<List<TimesheetEntity>> GetTimesheetsAsync(Guid userId, List<DateTime> timesheetDates);

        /// <summary>
        /// Gets the timesheet requests using project Id.
        /// </summary>
        /// <param name="projectId">The project Id of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// /// <param name="startDate">Start date of the month.</param>
        /// <param name="endDate">Last date the of the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        IEnumerable<TimesheetEntity> GetTimesheetRequestsByProjectId(Guid projectId, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the timesheet requests using project Id.
        /// </summary>
        /// <param name="projectIds">The project Ids of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// /// <param name="startDate">Start date of the month.</param>
        /// <param name="endDate">Last date the of the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        IEnumerable<TimesheetEntity> GetTimesheetRequestsByProjectIds(IEnumerable<Guid> projectIds, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the timesheet requests using project Id.
        /// </summary>
        /// <param name="taskId">The task Id of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// /// <param name="startDate">Start date of the month.</param>
        /// <param name="endDate">Last date the of the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetRequestsByTaskId(Guid taskId, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the timesheets of logged-in user for specified dates.
        /// </summary>
        /// <param name="timesheetDates">The dates of which timesheet needs to be retrieved.</param>
        /// <param name="userObjectId">The user object Id of which timesheets to get.</param>
        /// <param name="projectIds">The projects Ids of which timesheets to get.</param>
        /// <returns>Returns the collection of timesheet.</returns>
        IEnumerable<TimesheetEntity> GetTimesheetsOfUser(IEnumerable<DateTime> timesheetDates, Guid userObjectId, IEnumerable<Guid> projectIds = null);

        /// <summary>
        /// Gets filled timesheets by user within specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="userObjectId">The user Id of which projects to get.</param>
        /// <returns>Returns list of timesheet.</returns>
        Task<List<TimesheetEntity>> GetTimesheetsAsync(DateTime calendarStartDate, DateTime calendarEndDate, Guid userObjectId);

        /// <summary>
        /// Gets the timesheets of a date filled by user for tasks.
        /// </summary>
        /// <param name="timesheetDate">The timesheet date.</param>
        /// <param name="taskIds">The task Ids.</param>
        /// <param name="userObjectId">The user object Id.</param>
        /// <returns>The timesheets filled for tasks by user for date.</returns>
        IEnumerable<TimesheetEntity> GetTimesheets(DateTime timesheetDate, IEnumerable<Guid> taskIds, Guid userObjectId);

        /// <summary>
        /// Gets the timesheets of an user for specified date range.
        /// </summary>
        /// <param name="startDate">The start date from which timesheets to be retrieved.</param>
        /// <param name="endDate">The end date up to which timesheets to be retrieved.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns the collection of timesheets.</returns>
        IEnumerable<TimesheetEntity> GetTimesheetsOfUser(DateTime startDate, DateTime endDate, Guid userObjectId);

        /// <summary>
        /// Gets the submitted timesheets of a reportee.
        /// </summary>
        /// <param name="userObjectIds">The user Ids of which timesheets to get.</param>
        /// <param name="status">Timesheet status for filtering.</param>
        /// <returns>Returns user id and list of timesheets key value pairs.</returns>
        Dictionary<Guid, IEnumerable<TimesheetEntity>> GetTimesheetOfUsersByStatus(List<Guid> userObjectIds, TimesheetStatus status);

        /// <summary>
        /// Gets the submitted timesheets.
        /// </summary>
        /// <param name="managerId">The manager Id who created project.</param>
        /// <param name="timesheetIds">Timesheet Ids to fetch respective details.</param>
        /// <returns>Returns the list of timesheets.</returns>
        IEnumerable<TimesheetEntity> GetSubmittedTimesheetByIds(Guid managerId, IEnumerable<Guid> timesheetIds);

        /// <summary>
        /// Gets the timesheet by manager Id.
        /// </summary>
        /// <param name="managerId">The manager Id of the project's creator for which timesheets to get.</param>
        /// <param name="timesheetStatus">The status of timesheets to get.</param>
        /// <returns>Returns user id and list of timesheets key value pairs.</returns>
        Dictionary<Guid, IEnumerable<TimesheetEntity>> GetTimesheetsByManagerId(Guid managerId, TimesheetStatus timesheetStatus);
    }
}