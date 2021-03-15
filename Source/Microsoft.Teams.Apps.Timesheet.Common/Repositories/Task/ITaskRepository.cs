// <copyright file="ITaskRepository.cs" company="Microsoft Corporation">
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
    /// Exposes methods which will be used to perform database operations on task entity.
    /// </summary>
    public interface ITaskRepository : IBaseRepository<TaskEntity>
    {
        /// <summary>
        /// Creates a new task entry in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to save.</param>
        /// <returns>Returns whether operation is successful or not</returns>
        Task CreateTasksAsync(IEnumerable<TaskEntity> tasks);

        /// <summary>
        /// Updates task entries in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to update.</param>
        void UpdateTasks(IEnumerable<TaskEntity> tasks);

        /// <summary>
        /// Get active tasks of project.
        /// </summary>
        /// <param name="projectId">The project id of which tasks needs to be retrieved.</param>
        /// <returns>Returns the list of tasks.</returns>
        Task<IEnumerable<TaskEntity>> GetTasksByProjectIdAsync(Guid projectId);

        /// <summary>
        /// Gets task details including member.
        /// </summary>
        /// <param name="taskId">The task Id to get.</param>
        /// <returns>Returns the task details.</returns>
        TaskEntity GetTask(Guid taskId);
    }
}