// <copyright file="ManagerDashboardMapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microsoft.Teams.Apps.Timesheet.Test")]

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// A model class that contains methods related to manager dashboard model mappings.
    /// </summary>
    public class ManagerDashboardMapper : IManagerDashboardMapper
    {
        /// <summary>
        /// Gets timesheet view model to be sent as API response.
        /// </summary>
        /// <param name="timesheetsCollection">Collection of list of timesheet entity model.</param>
        /// <returns>Returns a timesheet view entity model.</returns>
        public IEnumerable<DashboardRequestDTO> MapForViewModel(IEnumerable<IEnumerable<TimesheetEntity>> timesheetsCollection)
        {
            timesheetsCollection = timesheetsCollection ?? throw new ArgumentNullException(nameof(timesheetsCollection));
            var dashboardRequests = timesheetsCollection.Select(timesheetCollectionItem => new DashboardRequestDTO
            {
                NumberOfDays = timesheetCollectionItem.GroupBy(timesheetRequest => timesheetRequest.TimesheetDate).Count(),
                TotalHours = timesheetCollectionItem.Sum(timesheet => timesheet.Hours),
                UserId = timesheetCollectionItem.First().UserId,
                Status = (int)timesheetCollectionItem.First().Status,
                UserName = string.Empty,
                RequestedForDates = this.GetGroupedDatesBySequence(timesheetCollectionItem),
                SubmittedTimesheetIds = timesheetCollectionItem.Select(timesheet => timesheet.Id),
            });

            return dashboardRequests;
        }

        /// <summary>
        /// Gets dashboard project view model to be sent as API response.
        /// </summary>
        /// <param name="project">The project entity model.</param>
        /// <param name="timesheets">List of timesheet entity model.</param>
        /// <returns>Returns a dashboard project view entity model.</returns>
        public DashboardProjectDTO MapForDashboardProjectViewModel(Project project, IEnumerable<TimesheetEntity> timesheets)
        {
            project = project ?? throw new ArgumentNullException(nameof(project));
            timesheets = timesheets ?? throw new ArgumentNullException(nameof(timesheets));

            var dashboardProject = new DashboardProjectDTO
            {
                Id = project.Id,
                Title = project.Title,
                TotalHours = project.BillableHours + project.NonBillableHours,
                UtilizedHours = timesheets.Sum(timesheet => timesheet.Hours),
            };

            return dashboardProject;
        }

        /// <summary>
        /// Get groups of dates according to order.
        /// Ex. Given dates: 1,2,3,6,8,9. Then 3 groups will be formed as [1,2,3],[6],[8,9].
        /// </summary>
        /// <param name="timesheets">Details of timesheet for which date range to get.</param>
        /// <returns>Returns list of continuous date range list.</returns>
        internal List<List<DateTime>> GetGroupedDatesBySequence(IEnumerable<TimesheetEntity> timesheets)
        {
            var orderedDates = timesheets
                .Select(timesheet => timesheet.TimesheetDate.Date)
                .Distinct()
                .OrderBy(date => date.Date)
                .ToList();

            var dateRange = new List<List<DateTime>>();
            int currentItemIndex = 0;

            // Filter date as range.
            // If ordered dates are suppose : [1 JAN, 2 JAN, 3 JAN, 5 JAN] then output will be [[1 JAN, 2 JAN, 3 JAN],[5 JAN]].
            for (int i = 0; i < orderedDates.Count; i++)
            {
                // If i = 0, add date on the start index.
                if (i == 0)
                {
                    dateRange.Add(new List<DateTime>());
                    dateRange[currentItemIndex].Add(orderedDates[i]);
                }
                else
                {
                    // If date is continuous (example 1 JAN, 2 JAN, 3 JAN), add them at same index.
                    if (orderedDates[i].Date.AddDays(-1) == dateRange[currentItemIndex].Last().Date)
                    {
                        dateRange[currentItemIndex].Add(orderedDates[i]);
                    }

                    // Else, add them at next index.
                    else
                    {
                        currentItemIndex += 1;
                        dateRange.Add(new List<DateTime>());
                        dateRange[currentItemIndex].Add(orderedDates[i]);
                    }
                }
            }

            return dateRange;
        }
    }
}