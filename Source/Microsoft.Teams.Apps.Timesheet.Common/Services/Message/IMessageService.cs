// <copyright file="IMessageService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Services.Message
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Teams message service.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Send message to a conversation.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="conversationId">Conversation Id.</param>
        /// <param name="serviceUrl">The service URL to use for sending the notification.</param>
        /// <param name="maxAttempts">Max attempts to send the message.</param>
        /// <param name="logger">Logger.</param>
        /// <returns>Send message response.</returns>
        public Task SendMessageAsync(
            IMessageActivity message,
            string conversationId,
            Uri serviceUrl,
            int maxAttempts,
            ILogger logger);
    }
}
