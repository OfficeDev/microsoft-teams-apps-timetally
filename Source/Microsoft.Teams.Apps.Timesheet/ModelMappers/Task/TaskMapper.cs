// <copyright file="TaskMapper.cs" company="Microsoft Corporation">
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
    /// This class manages model mappings related to task entity.
    /// </summary>
    public class TaskMapper : ITaskMapper
    {
        /// <summary>
        /// Gets the task model to be inserted in database.
        /// </summary>
        /// <param name="timesheetViewModel">The timesheet view model.</param>
        /// <param name="projectId">The project Id.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns task entity model.</returns>
        public TaskEntity MapForCreateModel(TimesheetDetails timesheetViewModel, Guid projectId, Guid userObjectId)
        {
            timesheetViewModel = timesheetViewModel ?? throw new ArgumentNullException(nameof(timesheetViewModel));

            var task = new TaskEntity
            {
                Title = timesheetViewModel.TaskTitle,
                IsAddedByMember = timesheetViewModel.IsAddedByMember,
                StartDate = timesheetViewModel.StartDate.Date,
                EndDate = timesheetViewModel.EndDate.Date,
                ProjectId = projectId,
            };

            return task;
        }

        /// <summary>
        /// Maps the task entity model to task view model.
        /// </summary>
        /// <param name="taskDetails">The task entity model.</param>
        /// <returns>Returns task view model.</returns>
        public TaskDTO MapForViewModel(TaskEntity taskDetails)
        {
            taskDetails = taskDetails ?? throw new ArgumentNullException(nameof(taskDetails), "The task details should not be null");

            return new TaskDTO
            {
                Id = taskDetails.Id,
                Title = taskDetails.Title,
                IsAddedByMember = taskDetails.IsAddedByMember,
                StartDate = taskDetails.StartDate.Date,
                EndDate = taskDetails.EndDate.Date,
                ProjectId = taskDetails.ProjectId,
            };
        }

        /// <summary>
        /// Gets task model to be inserted in database.
        /// </summary>
        /// <param name="projectId">The Id of the project in which tasks need to be created.</param>
        /// <param name="tasksViewModel">Tasks entity view model.</param>
        /// <returns>Returns list of task model.</returns>
        public IEnumerable<TaskEntity> MapForCreateModel(Guid projectId, IEnumerable<TaskDTO> tasksViewModel)
        {
            var tasks = tasksViewModel.Select(task => new TaskEntity
            {
                IsRemoved = false,
                ProjectId = projectId,
                Title = task.Title,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                IsAddedByMember = false,
            });

            return tasks;
        }

        /// <summary>
        /// Get tasks overview for a project.
        /// Overview contains tasks information along with burned efforts.
        /// </summary>
        /// <param name="tasks">List of tasks entity model.</param>
        /// <param name="timesheets">List of timesheet entity model.</param>
        /// <returns>Returns a list of project tasks overview view entity model.</returns>
        public IEnumerable<ProjectTaskOverviewDTO> MapForProjectTasksViewModel(IEnumerable<TaskEntity> tasks, IEnumerable<TimesheetEntity> timesheets)
        {
            tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            timesheets = timesheets ?? throw new ArgumentNullException(nameof(timesheets));

            var projectTasksOverview = tasks.Select(task => new ProjectTaskOverviewDTO
            {
                Id = task.Id,
                TotalHours = timesheets
                    .Where(timesheet => timesheet.TaskId == task.Id)
                    .Sum(timesheet => timesheet.Hours),
                Title = task.Title,
                IsRemoved = false,
            });

            return projectTasksOverview;
        }
    }
}
