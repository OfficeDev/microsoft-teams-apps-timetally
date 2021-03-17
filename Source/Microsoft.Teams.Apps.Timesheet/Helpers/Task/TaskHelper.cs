// <copyright file="TaskHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;

    /// <summary>
    /// Helper class which manages operations on project tasks.
    /// </summary>
    public class TaskHelper : ITaskHelper
    {
        /// <summary>
        /// The instance of repository accessors to access particular repository.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessor;

        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskHelper"/> class.
        /// </summary>
        /// <param name="repositoryAccessor">The instance of repository accessors to access repositories.</param>
        /// <param name="logger">Logs errors and information.</param>
        public TaskHelper(IRepositoryAccessors repositoryAccessor, ILogger<TaskHelper> logger)
        {
            this.repositoryAccessor = repositoryAccessor;
            this.logger = logger;
        }

        /// <summary>
        /// Adds new member task.
        /// </summary>
        /// <param name="taskDetails">The task details to be added.</param>
        /// <param name="projectId">The project Id.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns new task details if task created successfully. Else return null.</returns>
        public async Task<TaskEntity> AddMemberTaskAsync(TaskEntity taskDetails, Guid projectId, Guid userObjectId)
        {
            taskDetails = taskDetails ?? throw new ArgumentNullException(nameof(taskDetails), "The task details should not be null.");

            var projectMembers = await this.repositoryAccessor.MemberRepository.GetAllActiveMembersAsync(projectId);
            var memberDetails = projectMembers.ToList().Find(member => member.UserId == userObjectId);

            taskDetails.MemberMappingId = memberDetails.Id;
            taskDetails.StartDate = taskDetails.StartDate.Date;
            taskDetails.EndDate = taskDetails.EndDate.Date;

            var createdTaskDetails = this.repositoryAccessor.TaskRepository.Add(taskDetails);
            if (await this.repositoryAccessor.SaveChangesAsync() > 0)
            {
                this.logger.LogInformation("Task added successfully");
                return createdTaskDetails;
            }
            else
            {
                this.logger.LogInformation("Error occurred while adding new task");
                return null;
            }
        }

        /// <summary>
        /// Deletes a task created by project member.
        /// </summary>
        /// <param name="taskId">The task Id to be deleted.</param>
        /// <returns>Returns true if task deleted successfully. Else return false.</returns>
        public async Task<bool> DeleteMemberTaskAsync(Guid taskId)
        {
            var taskDetails = this.repositoryAccessor.TaskRepository.GetTask(taskId);
            taskDetails.IsRemoved = true;

            this.repositoryAccessor.TaskRepository.Update(taskDetails);
            if (await this.repositoryAccessor.SaveChangesAsync() > 0)
            {
                return true;
            }

            this.logger.LogInformation("Error occurred while deleting task");
            return false;
        }
    }
}
