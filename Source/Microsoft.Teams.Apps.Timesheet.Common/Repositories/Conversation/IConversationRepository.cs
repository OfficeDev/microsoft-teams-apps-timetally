// <copyright file="IConversationRepository.cs" company="Microsoft Corporation">
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
    /// Exposes methods which will be used to perform database operations on user conversation entity.
    /// </summary>
    public interface IConversationRepository : IBaseRepository<Conversation>
    {
        /// <summary>
        /// Get the conversations using user Id.
        /// </summary>
        /// <param name="userId">The user Id of which conversations to get.</param>
        /// <returns>Returns the collection of conversations.</returns>
        Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(Guid userId);
    }
}