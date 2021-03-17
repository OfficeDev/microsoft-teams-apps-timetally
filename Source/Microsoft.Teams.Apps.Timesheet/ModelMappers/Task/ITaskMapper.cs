// <copyright file="ITaskMapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Exposes methods that manages model mappings related to task entity.
    /// </summary>
    public interface ITaskMapper
    {
        /// <summary>
        /// Gets the task model to be inserted in database.
        /// </summary>
        /// <param name="timesheetViewModel">The timesheet view model.</param>
        /// <param name="projectId">The project Id.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns task entity model.</returns>
        TaskEntity MapForCreateModel(TimesheetDetails timesheetViewModel, Guid projectId, Guid userObjectId);

        /// <summary>
        /// Maps the task entity model to task view model.
        /// </summary>
        /// <param name="taskDetails">The task entity model.</param>
        /// <returns>Returns task view model.</returns>
        TaskDTO MapForViewModel(TaskEntity taskDetails);

        /// <summary>
        /// Gets task model to be inserted in database.
        /// </summary>
        /// <param name="projectId">The Id of the project in which tasks need to be created.</param>
        /// <param name="tasksViewModel">Tasks entity view model.</param>
        /// <returns>Returns list of task model.</returns>
        IEnumerable<TaskEntity> MapForCreateModel(Guid projectId, IEnumerable<TaskDTO> tasksViewModel);

        /// <summary>
        /// Gets project tasks overview view model to be sent as API response.
        /// </summary>
        /// <param name="tasks">List of tasks entity model.</param>
        /// <param name="timesheets">List of timesheet entity model.</param>
        /// <returns>Returns a list of project tasks overview view entity model.</returns>
        IEnumerable<ProjectTaskOverviewDTO> MapForProjectTasksViewModel(IEnumerable<TaskEntity> tasks, IEnumerable<TimesheetEntity> timesheets);
    }
}