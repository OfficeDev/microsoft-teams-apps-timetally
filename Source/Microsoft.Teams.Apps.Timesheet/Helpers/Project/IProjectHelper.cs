// <copyright file="IProjectHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Provides helper methods for managing projects.
    /// </summary>
    public interface IProjectHelper
    {
        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <param name="projectDetails">The project details.</param>
        /// <param name="userObjectId">The user object Id of project creator.</param>
        /// <returns>Returns project details.</returns>
        Task<ProjectDTO> CreateProjectAsync(ProjectDTO projectDetails, Guid userObjectId);

        /// <summary>
        /// Updates the details of a project.
        /// </summary>
        /// <param name="project">The project details that need to be updated.</param>
        /// <param name="userObjectId">The user object id who is going to update a project.</param>
        /// <returns>Return true if project is updated, else return false.</returns>
        Task<bool> UpdateProjectAsync(ProjectUpdateDTO project, Guid userObjectId);

        /// <summary>
        /// Gets a project by Id.
        /// </summary>
        /// <param name="projectId">The project Id of the project to fetch.</param>
        /// <param name="userObjectId">The user object Id of project creator.</param>
        /// <returns>Returns project details.</returns>
        Task<ProjectDTO> GetProjectByIdAsync(Guid projectId, Guid userObjectId);

        /// <summary>
        /// Get project utilization details between date range.
        /// </summary>
        /// <param name="projectId">The project Id of which details to fetch.</param>
        /// <param name="managerId">The manger Id who created the project.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns project utilization detail.</returns>
        Task<ProjectUtilizationDTO> GetProjectUtilizationAsync(Guid projectId, string managerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Add users in project.
        /// </summary>
        /// <param name="projectId">The Id of the project in which members need to be added.</param>
        /// <param name="membersToAdd">The list of members to be added.</param>
        /// <returns>Return true if project members are added, else return false.</returns>
        Task<bool> AddProjectMembersAsync(Guid projectId, IEnumerable<MemberDTO> membersToAdd);

        /// <summary>
        /// Create tasks in project.
        /// </summary>
        /// <param name="projectId">The Id of the project in which tasks need to be created.</param>
        /// <param name="tasks">The list of tasks details to be created.</param>
        /// <returns>Returns true if tasks are added, else false.</returns>
        Task<bool> AddProjectTasksAsync(Guid projectId, IEnumerable<TaskDTO> tasks);

        /// <summary>
        /// Delete members from a project.
        /// </summary>
        /// <param name="membersToDelete">The list of members to be deleted.</param>
        /// <returns>Returns true if members are deleted, else false.</returns>
        Task<bool> DeleteProjectMembersAsync(IEnumerable<Member> membersToDelete);

        /// <summary>
        /// Delete tasks from a project.
        /// </summary>
        /// <param name="tasksToDelete">The list of tasks to be deleted.</param>
        /// <returns>Returns true if tasks are deleted, else false.</returns>
        Task<bool> DeleteProjectTasksAsync(IEnumerable<TaskEntity> tasksToDelete);

        /// <summary>
        /// Get members overview for a project.
        /// Overview contains member information along with burned efforts.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of project members overview.</returns>
        Task<IEnumerable<ProjectMemberOverviewDTO>> GetProjectMembersOverviewAsync(Guid projectId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get tasks overview for a project.
        /// Overview contains task information along with burned efforts.
        /// </summary>
        /// <param name="projectId">The project Id of which details to fetch.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of project tasks overview.</returns>
        Task<IEnumerable<ProjectTaskOverviewDTO>> GetProjectTasksOverviewAsync(Guid projectId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get members of a project.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <param name="memberIds">Ids of member.</param>
        /// <returns>Returns null if all members doesn't belongs to project, else return members.</returns>
        Task<IEnumerable<Member>> GetProjectMembersAsync(Guid projectId, IEnumerable<Guid> memberIds);

        /// <summary>
        /// Get tasks of a project.
        /// </summary>
        /// <param name="projectId">The project Id of which tasks to fetch.</param>
        /// <param name="taskIds">Ids of tasks.</param>
        /// <returns>Returns null if all tasks doesn't belongs to project, else return tasks.</returns>
        Task<IEnumerable<TaskEntity>> GetProjectTasksAsync(Guid projectId, IEnumerable<Guid> taskIds);
    }
}