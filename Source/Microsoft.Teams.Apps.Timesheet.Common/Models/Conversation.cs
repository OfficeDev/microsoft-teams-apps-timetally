// <copyright file="Conversation.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;

    /// <summary>
    /// Represents Bot-user conversation details.
    /// </summary>
    public partial class Conversation
    {
        /// <summary>
        /// Gets or sets the user object Id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets Id of conversation between user and bot.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets service URL.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets date on which bot was installed for user.
        /// </summary>
        public DateTime BotInstalledOn { get; set; }
    }
}
