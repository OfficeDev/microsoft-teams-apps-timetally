// <copyright file="IMemberRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Exposes methods that will be used to manage operations on conversation entity.
    /// </summary>
    public interface IMemberRepository : IBaseRepository<Member>
    {
        /// <summary>
        /// Add users entries.
        /// </summary>
        /// <param name="users">The list of users entries to be added.</param>
        /// <returns>Returns whether the operation is successful or not</returns>
        Task AddUsersAsync(IEnumerable<Member> users);

        /// <summary>
        /// Gets active members of project.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <returns>Return list of members entity model.</returns>
        Task<IEnumerable<Member>> GetAllActiveMembersAsync(Guid projectId);

        /// <summary>
        /// Gets all members of project.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <returns>Return list of members entity model.</returns>
        List<Member> GetAllMembers(Guid projectId);

        /// <summary>
        /// Updates the details of a members.
        /// </summary>
        /// <param name="members">The members details that need to be updated.</param>
        void UpdateMembers(IEnumerable<Member> members);
    }
}