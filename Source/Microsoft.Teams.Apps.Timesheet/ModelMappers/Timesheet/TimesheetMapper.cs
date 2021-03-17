// <copyright file="TimesheetMapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// This class manages model mappings related to timesheet entity.
    /// </summary>
    public class TimesheetMapper : ITimesheetMapper
    {
        /// <summary>
        /// Gets the timesheet model to be inserted in database.
        /// </summary>
        /// <param name="timesheetDate">The timesheet date to be save.</param>
        /// <param name="timesheetViewModel">The timesheet view model.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>The timesheet entity model.</returns>
        public TimesheetEntity MapForCreateModel(DateTime timesheetDate, TimesheetDetails timesheetViewModel, Guid userObjectId)
        {
            timesheetViewModel = timesheetViewModel ?? throw new ArgumentNullException(nameof(timesheetViewModel));

            var timesheet = new TimesheetEntity
            {
                TaskId = timesheetViewModel.TaskId,
                TaskTitle = timesheetViewModel.TaskTitle,
                TimesheetDate = timesheetDate,
                Hours = timesheetViewModel.Hours,
                Status = timesheetViewModel.Status,
                UserId = userObjectId,
            };

            if (timesheetViewModel.Status == (int)TimesheetStatus.Submitted)
            {
                timesheet.SubmittedOn = DateTime.UtcNow;
            }
            else
            {
                timesheet.SubmittedOn = null;
            }

            return timesheet;
        }

        /// <summary>
        /// Maps timesheet view model details to timesheet entity model that to be updated in database.
        /// </summary>
        /// <param name="timesheetViewModel">The timesheet entity view model.</param>
        /// <param name="timesheetModel">The timesheet entity model.</param>
        public void MapForUpdateModel(TimesheetDetails timesheetViewModel, TimesheetEntity timesheetModel)
        {
            timesheetViewModel = timesheetViewModel ?? throw new ArgumentNullException(nameof(timesheetViewModel));
            timesheetModel = timesheetModel ?? throw new ArgumentNullException(nameof(timesheetModel));

            timesheetModel.Status = timesheetViewModel.Status;
            timesheetModel.Hours = timesheetViewModel.Hours;
            timesheetModel.LastModifiedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Maps timesheet database entity to view model.
        /// </summary>
        /// <param name="timesheet">The timesheet details.</param>
        /// <returns>Returns timesheet view model.</returns>
        public TimesheetDTO MapForViewModel(TimesheetEntity timesheet)
        {
            timesheet = timesheet ?? throw new ArgumentNullException(nameof(timesheet), "Timesheet details should not be null");

            return new TimesheetDTO
            {
                Id = timesheet.Id,
                TaskTitle = timesheet.TaskTitle,
                TimesheetDate = timesheet.TimesheetDate.Date,
                Hours = timesheet.Hours,
                Status = timesheet.Status,
            };
        }

        /// <summary>
        /// Gets request approval view model to be sent as API response.
        /// </summary>
        /// <param name="timesheets">List of submitted timesheets.</param>
        /// <returns>Returns a submitted request DTO view entity model.</returns>
        public IEnumerable<SubmittedRequestDTO> MapToViewModel(IEnumerable<TimesheetEntity> timesheets)
        {
            timesheets = timesheets ?? throw new ArgumentNullException(nameof(timesheets));

            var userTimesheets = timesheets.GroupBy(timesheet => timesheet.TimesheetDate).Select(timesheetsGroup => new SubmittedRequestDTO
            {
                TotalHours = timesheetsGroup.Sum(timesheet => timesheet.Hours),
                UserId = timesheetsGroup.First().UserId,
                Status = timesheetsGroup.First().Status,
                TimesheetDate = timesheetsGroup.First().TimesheetDate,
                ProjectTitles = timesheetsGroup
                    .Select(timesheet => timesheet.Task.Project.Title.Trim())
                    .Distinct(),
                SubmittedTimesheetIds = timesheetsGroup.Select(timesheet => timesheet.Id),
            });

            return userTimesheets;
        }
    }
}
