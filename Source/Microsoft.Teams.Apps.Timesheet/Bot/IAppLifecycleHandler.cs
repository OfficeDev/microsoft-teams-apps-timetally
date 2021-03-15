// <copyright file="IAppLifecycleHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Bot
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Helper for handling bot related activities.
    /// </summary>
    public interface IAppLifecycleHandler
    {
        /// <summary>
        /// Sends welcome card to user when bot is installed in personal scope.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        Task OnBotInstalledInPersonalAsync(ITurnContext<IConversationUpdateActivity> turnContext);
    }
}