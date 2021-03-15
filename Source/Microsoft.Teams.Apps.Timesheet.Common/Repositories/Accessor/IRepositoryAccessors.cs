// <copyright file="IRepositoryAccessors.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System.Threading.Tasks;

    /// <summary>
    /// Exposes repository instances that can be used to perform database operations.
    /// </summary>
    public interface IRepositoryAccessors
    {
        /// <summary>
        /// Gets instance of project repository.
        /// </summary>
        public IProjectRepository ProjectRepository { get; }

        /// <summary>
        /// Gets instance of timesheet repository.
        /// </summary>
        public ITimesheetRepository TimesheetRepository { get; }

        /// <summary>
        /// Gets instance of task repository.
        /// </summary>
        public ITaskRepository TaskRepository { get; }

        /// <summary>
        /// Gets instance of user project mapping repository.
        /// </summary>
        public IMemberRepository MemberRepository { get; }

        /// <summary>
        /// Gets instance of user conversation repository.
        /// </summary>
        public IConversationRepository ConversationRepository { get; }

        /// <summary>
        /// Gets the timesheet context.
        /// </summary>
        public TimesheetContext Context { get; }

        /// <summary>
        /// Saves changes made till now to database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<int> SaveChangesAsync();
    }
}