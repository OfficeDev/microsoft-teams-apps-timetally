// <copyright file="TimesheetRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// This class manages all database operations related to timesheet entity.
    /// </summary>
    public class TimesheetRepository : BaseRepository<TimesheetEntity>, ITimesheetRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimesheetRepository"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        public TimesheetRepository(TimesheetContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Updates timesheet entries.
        /// </summary>
        /// <param name="timesheets">The list of timesheet entries to be updated.</param>
        public void Update(IEnumerable<TimesheetEntity> timesheets)
        {
            this.Context.Timesheets.UpdateRange(timesheets);
        }

        /// <summary>
        /// Gets the timesheet requests using project id
        /// </summary>
        /// <param name="projectId">The project id of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// <param name="startDate">Start date of the month.</param>
        /// <param name="endDate">Last date the of the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetRequestsByProjectId(Guid projectId, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate)
        {
            var status = (int)timesheetStatus;

            return this.Context.Timesheets
                .Where(timesheet => timesheet.Task.ProjectId == projectId && timesheet.Status == status && timesheet.TimesheetDate >= startDate.Date && timesheet.TimesheetDate.Date <= endDate.Date)
                .Include(timesheet => timesheet.Task);
        }

        /// <summary>
        /// Gets the timesheet requests using project Id.
        /// </summary>
        /// <param name="projectIds">The project Ids of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// /// <param name="startDate">Start date of the month.</param>
        /// <param name="endDate">Last date the of the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetRequestsByProjectIds(IEnumerable<Guid> projectIds, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate)
        {
            var status = (int)timesheetStatus;

            return this.Context.Timesheets
                .Include(timesheet => timesheet.Task)
                .Where(timesheet => projectIds.Contains(timesheet.Task.ProjectId)
                    && timesheet.Status == status
                    && timesheet.TimesheetDate >= startDate.Date
                    && timesheet.TimesheetDate.Date <= endDate.Date
                    && !timesheet.Task.IsRemoved);
        }

        /// <summary>
        /// Gets the  timesheet requests using task id
        /// </summary>
        /// <param name="taskId">The task id of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// <param name="startDate">Start date of the month.</param>
        /// <param name="endDate">Last date the of the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetRequestsByTaskId(Guid taskId, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate)
        {
            return this.Context.Timesheets
                .Where(timesheet => timesheet.TaskId == taskId && timesheet.Status == (int)timesheetStatus
                && timesheet.TimesheetDate >= startDate.Date && timesheet.TimesheetDate.Date <= endDate.Date);
        }

        /// <summary>
        /// Gets the timesheets of logged-in user for specified dates.
        /// </summary>
        /// <param name="timesheetDates">The dates of which timesheet needs to be retrieved.</param>
        /// <param name="userObjectId">The user object Id of which timesheets to get.</param>
        /// <param name="projectIds">The projects Ids of which timesheets to get.</param>
        /// <returns>Returns the collection of timesheet.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetsOfUser(IEnumerable<DateTime> timesheetDates, Guid userObjectId, IEnumerable<Guid> projectIds = null)
        {
            var timesheets = this.Context.Timesheets
                .Where(timesheet => timesheet.UserId.Equals(userObjectId) && timesheetDates.Contains(timesheet.TimesheetDate))
                .Include(timesheet => timesheet.Task)
                .Where(timesheet => !timesheet.Task.IsRemoved)
                .AsEnumerable();

            if (!projectIds.IsNullOrEmpty())
            {
                timesheets = timesheets
                    .Where(timesheet => projectIds.Contains(timesheet.Task.ProjectId)) ?? new List<TimesheetEntity>();
            }

            return timesheets;
        }

        /// <summary>
        /// Gets filled timesheets by user within specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="userObjectId">The user Id of which projects to get.</param>
        /// <returns>Returns list of timesheet.</returns>
        public async Task<List<TimesheetEntity>> GetTimesheetsAsync(DateTime calendarStartDate, DateTime calendarEndDate, Guid userObjectId)
        {
            return await this.Context.Timesheets
                .Where(timesheet => timesheet.UserId.Equals(userObjectId)
                    && timesheet.TimesheetDate >= calendarStartDate
                    && timesheet.TimesheetDate <= calendarEndDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets the timesheets of a date filled by user for tasks.
        /// </summary>
        /// <param name="timesheetDate">The timesheet date.</param>
        /// <param name="taskIds">The task Ids.</param>
        /// <param name="userObjectId">The user object Id.</param>
        /// <returns>The timesheets filled for tasks by user for date.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheets(DateTime timesheetDate, IEnumerable<Guid> taskIds, Guid userObjectId)
        {
            return this.Context.Timesheets
                .Where(timesheet => timesheet.UserId == userObjectId
                    && timesheet.TimesheetDate == timesheetDate
                    && taskIds.Contains(timesheet.TaskId));
        }

        /// <summary>
        /// Gets the timesheets of an user for specified date range.
        /// </summary>
        /// <param name="startDate">The start date from which timesheets to be retrieved.</param>
        /// <param name="endDate">The end date up to which timesheets to be retrieved.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns the collection of timesheets.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetsOfUser(DateTime startDate, DateTime endDate, Guid userObjectId)
        {
            return this.Context.Timesheets
                .Include(timesheet => timesheet.Task)
                .Where(timesheet => timesheet.UserId == userObjectId
                    && !timesheet.Task.IsRemoved
                    && timesheet.TimesheetDate >= startDate.Date
                    && timesheet.TimesheetDate <= endDate.Date);
        }

        /// <summary>
        /// Gets the submitted timesheets.
        /// </summary>
        /// <param name="managerId">The manager Id who created project.</param>
        /// <param name="timesheetIds">Timesheet Ids to fetch respective details.</param>
        /// <returns>Returns the list of timesheets.</returns>
        public IEnumerable<TimesheetEntity> GetSubmittedTimesheetByIds(Guid managerId, IEnumerable<Guid> timesheetIds)
        {
            return this.Context.Timesheets
                .Where(timesheet => timesheetIds.Contains(timesheet.Id) &&
                    timesheet.Status == (int)TimesheetStatus.Submitted &&
                    timesheet.Task.Project.CreatedBy == managerId)
                .Include(timesheet => timesheet.Task)
                .Include(timesheet => timesheet.Task.Project);
        }

        /// <summary>
        /// Gets the timesheet by manager Id.
        /// </summary>
        /// <param name="managerId">The manager Id of the project's creator for which timesheets to get.</param>
        /// <param name="timesheetStatus">The status of timesheets to get.</param>
        /// <returns>Returns user id and list of timesheets key value pairs.</returns>
        public Dictionary<Guid, IEnumerable<TimesheetEntity>> GetTimesheetsByManagerId(Guid managerId, TimesheetStatus timesheetStatus)
        {
            return this.Context.Timesheets
                .Where(timesheet => timesheet.Status == (int)timesheetStatus && timesheet.Task.Project.CreatedBy == managerId && !timesheet.Task.IsRemoved)
                .AsEnumerable()
                .GroupBy(timesheet => timesheet.UserId)
                .ToDictionary(timesheet => timesheet.Key, timesheet => timesheet.AsEnumerable());
        }

        /// <summary>
        /// Gets the submitted timesheets of a reportee.
        /// </summary>
        /// <param name="userObjectIds">The user Ids of which timesheets to get.</param>
        /// <param name="status">Timesheet status for filtering.</param>
        /// <returns>Returns user id and list of timesheets key value pairs.</returns>
        public Dictionary<Guid, IEnumerable<TimesheetEntity>> GetTimesheetOfUsersByStatus(List<Guid> userObjectIds, TimesheetStatus status)
        {
            return this.Context.Timesheets
                .Where(timesheet => timesheet.Status == (int)status && userObjectIds.Contains(timesheet.UserId) && !timesheet.Task.IsRemoved)
                .Include(timesheet => timesheet.Task)
                .Include(timesheet => timesheet.Task.Project)
                .AsEnumerable()
                .GroupBy(timesheet => timesheet.UserId)
                .ToDictionary(timesheet => timesheet.Key, timesheet => timesheet.AsEnumerable());
        }

        /// <summary>
        /// Gets the timesheet requests.
        /// </summary>
        /// <param name="userId">The user Id of which requests to get.</param>
        /// <param name="timesheetDates">The dates of requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        public async Task<List<TimesheetEntity>> GetTimesheetsAsync(Guid userId, List<DateTime> timesheetDates)
        {
            return await this.Context.Timesheets
                .Where(timesheet => timesheetDates.Contains(timesheet.TimesheetDate) && timesheet.UserId == userId)
                .Include(timesheet => timesheet.Task)
                .Include(timesheet => timesheet.Task.Project)
                .ToListAsync();
        }
    }
}