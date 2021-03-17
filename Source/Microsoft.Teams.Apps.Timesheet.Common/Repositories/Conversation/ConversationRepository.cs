// <copyright file="ConversationRepository.cs" company="Microsoft Corporation">
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
    /// This class manages all database operations related to user conversation entity.
    /// </summary>
    public class ConversationRepository : BaseRepository<Conversation>, IConversationRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationRepository"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        public ConversationRepository(TimesheetContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Get the conversations using user Id.
        /// </summary>
        /// <param name="userId">The user Id of which conversations to get.</param>
        /// <returns>Returns the collection of conversations.</returns>
        public async Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(Guid userId)
        {
            return await this.FindAsync(conversation => conversation.UserId == userId);
        }
    }
}