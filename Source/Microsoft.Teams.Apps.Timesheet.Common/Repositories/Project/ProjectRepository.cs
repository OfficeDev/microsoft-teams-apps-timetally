// <copyright file="ProjectRepository.cs" company="Microsoft Corporation">
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
    /// This class manages all database operations related to project entity.
    /// </summary>
    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRepository"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        public ProjectRepository(TimesheetContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Creates new project.
        /// </summary>
        /// <param name="projectDetails">The project details to save.</param>
        /// <returns>Returns boolean indication whether create project was successful.</returns>
        public Project CreateProject(Project projectDetails)
        {
            var createdProject = this.Add(projectDetails);
            return createdProject;
        }

        /// <summary>
        /// Get all active projects created by manager.
        /// </summary>
        /// <param name="userObjectId">The user Id who created a project.</param>
        /// <returns>Returns list of projects.</returns>
        public IEnumerable<Project> GetActiveProjectsForManager(Guid userObjectId)
        {
            return this.Context.Projects
                .Where(project => project.CreatedBy == userObjectId && DateTime.UtcNow.Date >= project.StartDate.Date && DateTime.UtcNow.Date <= project.EndDate.Date)
                .OrderBy(project => project.CreatedOn);
        }

        /// <summary>
        /// Get all active projects whose start date is greater than and end date is less than current date.
        /// </summary>
        /// <param name="managerUserObjectId">The manager user object Id who created a project.</param>
        /// <returns>Returns list of projects.</returns>
        public async Task<IEnumerable<Project>> GetActiveProjectsAsync(Guid managerUserObjectId)
        {
            return await this.Context.Projects
                .Where(project => project.CreatedBy.Equals(managerUserObjectId)
                    && DateTime.UtcNow.Date >= project.StartDate.Date
                    && DateTime.UtcNow.Date <= project.EndDate.Date)
                .OrderBy(project => project.CreatedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Get all managers user object IDs.
        /// </summary>
        /// <returns>Returns the project details along with tasks and members details.</returns>
        public List<Guid> GetAllManagersUserIDs()
        {
            return this.Context.Projects.Select(project => project.CreatedBy).Distinct().ToList();
        }

        /// <inheritdoc/>
        public List<Project> GetProjectDetailByProjectIds(List<Guid> projectId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all active projects along with tasks assigned to user between specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="userObjectId">The user Id of which projects to get.</param>
        /// <returns>Returns all active projects assigned to user on particular date.</returns>
        public async Task<IEnumerable<Project>> GetProjectsAsync(DateTime calendarStartDate, DateTime calendarEndDate, Guid userObjectId)
        {
            return await this.Context.Projects
                .Where(project => project.Members.Where(member => member.UserId == userObjectId).Any()
                && ((project.StartDate.Date >= calendarStartDate && project.StartDate.Date <= calendarEndDate) || (project.StartDate.Date < calendarStartDate && project.EndDate.Date >= calendarStartDate)))
                .Include(project => project.Tasks)
                .Include(project => project.Members)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all active projects along with tasks assigned to user between specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="userObjectId">The user Id of which projects to get.</param>
        /// <returns>Returns all active projects assigned to user on particular date.</returns>
        public IEnumerable<UserTimesheet> GetProjects(DateTime calendarStartDate, DateTime calendarEndDate, Guid userObjectId)
        {
            // Get projects between specified start and end date along with task details.
            var projects = this.Context.Projects
                .Where(project => ((project.StartDate.Date >= calendarStartDate.Date && project.StartDate.Date <= calendarEndDate.Date) ||
                    (project.StartDate.Date < calendarStartDate.Date && project.EndDate.Date >= calendarStartDate.Date)) && project.Members.Where(member => member.UserId == userObjectId).Any())
                .Include(project => project.Tasks)
                .ToList();

            // Get timesheets of a user which were filled within specified start and end date.
            var filledTimesheets = this.Context.Timesheets
                .Where(timesheet => timesheet.UserId.Equals(userObjectId)
                && timesheet.TimesheetDate.Date >= calendarStartDate.Date
                && timesheet.TimesheetDate.Date <= calendarEndDate.Date)
                .ToList();

            return this.GetTimesheetsForDates(calendarStartDate, calendarEndDate, projects, filledTimesheets);
        }

        /// <summary>
        /// Get project details by project Id.
        /// </summary>
        /// <param name="projectId">The project Id of which details need to be retrieved.</param>
        /// <param name="userObjectId">The user object Id of manager who created a project.</param>
        /// <returns>Returns the project details along with tasks and members details.</returns>
        public async Task<Project> GetProjectByIdAsync(Guid projectId, Guid userObjectId)
        {
            var project = await this.Context.Projects
                .Where(project => project.Id.Equals(projectId) && project.CreatedBy.Equals(userObjectId))
                .Include(project => project.Tasks)
                .Include(project => project.Members)
                .FirstOrDefaultAsync();

            project.Tasks = project.Tasks.Where(task => !task.IsRemoved).ToList();
            project.Members = project.Members.Where(member => !member.IsRemoved).ToList();

            return project;
        }

        /// <summary>
        /// Get timesheets for dates for list of projects.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="projects">Projects between specified start and end date along with task details.</param>
        /// <param name="filledTimesheets">Timesheets of a user which were filled within specified start and end date</param>
        /// <returns>Returns list of timesheets.</returns>
        private List<UserTimesheet> GetTimesheetsForDates(DateTime calendarStartDate, DateTime calendarEndDate, List<Project> projects, List<TimesheetEntity> filledTimesheets)
        {
            var timesheetDetails = new List<UserTimesheet>();
            UserTimesheet timesheetData = null;

            // Iterate on total number of days between specified start and end date to get timesheet data of each day.
            for (int i = 0; i <= calendarEndDate.Subtract(calendarStartDate).TotalDays; i++)
            {
                timesheetData = new UserTimesheet
                {
                    TimesheetDate = calendarStartDate.AddDays(i).Date,
                };

                // Retrieves projects of particular calendar date ranges in specified start and end date.
                var filteredProjects = projects.Where(project => timesheetData.TimesheetDate >= project.StartDate && timesheetData.TimesheetDate <= project.EndDate);

                if (filteredProjects.IsNullOrEmpty())
                {
                    continue;
                }

                timesheetData.ProjectDetails = new List<ProjectDetails>();

                // Iterate on each project to get task and timesheet details.
                foreach (var project in filteredProjects)
                {
                    timesheetData.ProjectDetails.Add(new ProjectDetails
                    {
                        Id = project.Id,
                        Title = project.Title,
                        TimesheetDetails = project.Tasks.Select(task => new TimesheetDetails
                        {
                            TaskId = task.Id,
                            TaskTitle = task.Title,
                            Hours = filledTimesheets.Where(timesheet => timesheet.TaskId == task.Id && timesheet.TimesheetDate.Date == timesheetData.TimesheetDate.Date).ToList().Select(x => x.Hours).FirstOrDefault(),
                            ManagerComments = filledTimesheets.Where(timesheet => timesheet.TaskId == task.Id && timesheet.TimesheetDate.Date == timesheetData.TimesheetDate.Date).Select(x => x.ManagerComments).FirstOrDefault(),
                            Status = filledTimesheets.Where(timesheet => timesheet.TaskId == task.Id && timesheet.TimesheetDate.Date == timesheetData.TimesheetDate.Date).Select(x => x.Status).FirstOrDefault(),
                        }).ToList(),
                    });
                }

                timesheetDetails.Add(timesheetData);
            }

            return timesheetDetails;
        }
    }
}