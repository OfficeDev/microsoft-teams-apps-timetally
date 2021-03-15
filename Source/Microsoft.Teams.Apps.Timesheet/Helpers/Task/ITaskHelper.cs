// <copyright file="ITaskHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// Exposes helper methods required for managing tasks.
    /// </summary>
    public interface ITaskHelper
    {
        /// <summary>
        /// Adds new member task.
        /// </summary>
        /// <param name="taskDetails">The task details to be added.</param>
        /// <param name="projectId">The project Id.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns new task details if task created successfully. Else return null.</returns>
        Task<TaskEntity> AddMemberTaskAsync(TaskEntity taskDetails, Guid projectId, Guid userObjectId);

        /// <summary>
        /// Deletes a task created by project member.
        /// </summary>
        /// <param name="taskId">The task Id to be deleted.</param>
        /// <returns>Returns true if task deleted successfully. Else return false.</returns>
        Task<bool> DeleteMemberTaskAsync(Guid taskId);
    }
}
