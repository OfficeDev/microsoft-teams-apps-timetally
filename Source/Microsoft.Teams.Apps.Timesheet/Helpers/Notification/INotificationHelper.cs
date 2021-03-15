// <copyright file="INotificationHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Tasks = System.Threading.Tasks;

    /// <summary>
    /// Helper for sending notifications to users.
    /// </summary>
    public interface INotificationHelper
    {
        /// <summary>
        /// Sends notification to the user.
        /// </summary>
        /// <param name="user">The user to which notification need to send</param>
        /// <param name="card">The notification card that to be send</param>
        /// <returns>A <see cref="Tasks.Task"/> representing the asynchronous operation.</returns>
        Tasks.Task SendNotificationToUserAsync(Conversation user, Attachment card);
    }
}