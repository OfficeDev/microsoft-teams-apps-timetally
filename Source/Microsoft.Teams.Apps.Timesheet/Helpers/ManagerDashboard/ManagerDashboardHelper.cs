// <copyright file="ManagerDashboardHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;

    /// <summary>
    /// Provides helper methods for managing operations related to managers dashboard.
    /// </summary>
    public class ManagerDashboardHelper : IManagerDashboardHelper
    {
        /// <summary>
        /// The instance of timesheet DB context.
        /// </summary>
        private readonly TimesheetContext context;

        /// <summary>
        /// The instance of repository accessors to access repositories.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessors;

        /// <summary>
        /// The instance of user Graph service to access logged-in user's reportees and manager.
        /// </summary>
        private readonly IUsersService userGraphService;

        /// <summary>
        /// The instance of manager dashboard mapper.
        /// </summary>
        private readonly IManagerDashboardMapper managerDashboardMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerDashboardHelper"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        /// <param name="repositoryAccessors">The instance of repository accessors.</param>
        /// <param name="userGraphService">The instance of user Graph service to access logged-in user's reportees and manager.</param>
        /// <param name="managerDashboardMapper">The instance of manager dashboard mapper.</param>
        public ManagerDashboardHelper(TimesheetContext context, IRepositoryAccessors repositoryAccessors, IUsersService userGraphService, IManagerDashboardMapper managerDashboardMapper)
        {
            this.context = context;
            this.repositoryAccessors = repositoryAccessors;
            this.userGraphService = userGraphService;
            this.managerDashboardMapper = managerDashboardMapper;
        }

        /// <summary>
        /// Gets timesheets which are pending for manager approval.
        /// </summary>
        /// <param name="managerObjectId">The manager Id for which request has been raised.</param>
        /// <returns>Return list of submitted timesheets.</returns>
        public async Task<IEnumerable<DashboardRequestDTO>> GetDashboardRequestsAsync(Guid managerObjectId)
        {
            // Get timesheets submitted to manager.
            var timesheets = this.repositoryAccessors.TimesheetRepository.GetTimesheetsByManagerId(managerObjectId, TimesheetStatus.Submitted);

            if (!timesheets.Any())
            {
                return Enumerable.Empty<DashboardRequestDTO>();
            }

            var timesheetListCollection = timesheets.Select(timesheet => timesheet.Value);

            // Map timesheet entity to dashboard requests view model.
            var dashboardRequests = this.managerDashboardMapper.MapForViewModel(timesheetListCollection).ToList();

            var userIds = dashboardRequests.Select(dashboardRequest => dashboardRequest.UserId.ToString());
            var users = await this.userGraphService.GetUsersAsync(userIds);

            // Mapping users with their graph user display name.
            for (var i = 0; i < dashboardRequests.Count; i++)
            {
                if (users.TryGetValue(dashboardRequests[i].UserId, out var user))
                {
                    dashboardRequests[i].UserName = user.DisplayName;
                }
            }

            return dashboardRequests;
        }

        /// <summary>
        /// Get approved and active project details for dashboard between date range.
        /// </summary>
        /// <param name="managerUserObjectId">The manager user object Id who created a project.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of dashboard projects.</returns>
        public async Task<IEnumerable<DashboardProjectDTO>> GetDashboardProjectsAsync(Guid managerUserObjectId, DateTime startDate, DateTime endDate)
        {
            var projects = await this.repositoryAccessors.ProjectRepository.GetActiveProjectsAsync(managerUserObjectId);

            if (projects.IsNullOrEmpty())
            {
                return Enumerable.Empty<DashboardProjectDTO>();
            }

            var dashboardProjects = new List<DashboardProjectDTO>();
            var projectIds = projects.Select(project => project.Id);
            var timesheets = this.repositoryAccessors.TimesheetRepository.GetTimesheetRequestsByProjectIds(projectIds, TimesheetStatus.Approved, startDate, endDate);
            var timesheetDictionary = timesheets.GroupBy(timesheet => timesheet.Task.ProjectId).ToDictionary(group => group.Key);

            foreach (var project in projects)
            {
                var timesheetsToMap = Enumerable.Empty<TimesheetEntity>();
                if (timesheetDictionary.ContainsKey(project.Id))
                {
                    timesheetsToMap = timesheetDictionary[project.Id];
                }

                dashboardProjects
                    .Add(this.managerDashboardMapper.MapForDashboardProjectViewModel(project, timesheetsToMap));
            }

            return dashboardProjects;
        }
    }
}
