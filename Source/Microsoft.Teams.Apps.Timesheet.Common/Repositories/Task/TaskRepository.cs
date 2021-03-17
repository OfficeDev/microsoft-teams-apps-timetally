// <copyright file="TaskRepository.cs" company="Microsoft Corporation">
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
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// This class manages all database operations related to timesheet entity.
    /// </summary>
    public class TaskRepository : BaseRepository<TaskEntity>, ITaskRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskRepository"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        public TaskRepository(TimesheetContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Get active tasks of project.
        /// </summary>
        /// <param name="projectId">The project id of which tasks needs to be retrieved.</param>
        /// <returns>Returns the list of tasks.</returns>
        public async Task<IEnumerable<TaskEntity>> GetTasksByProjectIdAsync(Guid projectId)
        {
            return await this.Context.Tasks
                .Where(task => task.ProjectId == projectId && !task.IsRemoved)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new task entry in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to save.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task CreateTasksAsync(IEnumerable<TaskEntity> tasks)
        {
            await this.Context.AddRangeAsync(tasks);
        }

        /// <summary>
        /// Updates task entries in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to update.</param>
        public void UpdateTasks(IEnumerable<TaskEntity> tasks)
        {
            this.Context.Tasks.UpdateRange(tasks);
        }

        /// <summary>
        /// Gets task details including member.
        /// </summary>
        /// <param name="taskId">The task Id to get.</param>
        /// <returns>Returns the task details.</returns>
        public TaskEntity GetTask(Guid taskId)
        {
            return this.Context.Tasks
                .Where(task => task.Id == taskId)
                .Include(task => task.MemberMapping)
                .FirstOrDefault();
        }
    }
}