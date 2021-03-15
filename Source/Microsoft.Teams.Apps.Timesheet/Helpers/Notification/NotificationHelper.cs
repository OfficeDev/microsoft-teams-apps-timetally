// <copyright file="NotificationHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;
    using Tasks = System.Threading.Tasks;

    /// <summary>
    /// Helper for notification activities
    /// </summary>
    public class NotificationHelper : INotificationHelper
    {
        /// <summary>
        /// Default value for channel activity to send notifications
        /// </summary>
        private const string TeamsBotChannelId = "msteams";

        /// <summary>
        /// Represents retry count
        /// </summary>
        private const int RetryCount = 2;

        /// <summary>
        /// Instance of IBot framework HTTP adapter.
        /// </summary>
        private readonly IBotFrameworkHttpAdapter botFrameworkHttpAdapter;

        /// <summary>
        /// Holds the Microsoft app credentials
        /// </summary>
        private readonly MicrosoftAppCredentials microsoftAppCredentials;

        /// <summary>
        /// Instance of logger to log event and errors.
        /// </summary>
        private readonly ILogger<NotificationHelper> logger;

        /// <summary>
        /// Retry policy with jitter, retry twice with a jitter delay of up to 1.5 sec. Retry for HTTP 429(transient error)/502 bad gateway.
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
        /// </remarks>
        private readonly AsyncRetryPolicy retryPolicy = Policy.Handle<ErrorResponseException>(
            ex => ex.Response.StatusCode == HttpStatusCode.TooManyRequests || ex.Response.StatusCode == HttpStatusCode.BadGateway)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(1500), RetryCount));

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationHelper"/> class.
        /// </summary>
        /// <param name="botFrameworkHttpAdapter">The bot adapter</param>
        /// <param name="microsoftAppCredentials">The Microsoft app credentials</param>
        /// <param name="logger">Instance of logger to log event and errors</param>
        public NotificationHelper(
            IBotFrameworkHttpAdapter botFrameworkHttpAdapter,
            MicrosoftAppCredentials microsoftAppCredentials,
            ILogger<NotificationHelper> logger)
        {
            this.botFrameworkHttpAdapter = botFrameworkHttpAdapter;
            this.microsoftAppCredentials = microsoftAppCredentials;
            this.logger = logger;
        }

        /// <summary>
        /// Sends notification to the user.
        /// </summary>
        /// <param name="user">The user to which notification need to send</param>
        /// <param name="card">The notification card that to be send</param>
        /// <returns>A <see cref="Tasks.Task"/> representing the asynchronous operation.</returns>
        public async Tasks.Task SendNotificationToUserAsync(Conversation user, Attachment card)
        {
            if (user == null || string.IsNullOrEmpty(user.ConversationId)
                || string.IsNullOrEmpty(Convert.ToString(user.UserId, CultureInfo.InvariantCulture))
                || string.IsNullOrEmpty(user.ServiceUrl)
                || card == null)
            {
                return;
            }

            try
            {
                MicrosoftAppCredentials.TrustServiceUrl(user.ServiceUrl);

                var conversationReference = new ConversationReference()
                {
                    Bot = new ChannelAccount() { Id = $"28:{this.microsoftAppCredentials.MicrosoftAppId}" },
                    ChannelId = TeamsBotChannelId,
                    Conversation = new ConversationAccount() { Id = user.ConversationId },
                    ServiceUrl = user.ServiceUrl,
                };

                var botFrameworkAdapter = this.botFrameworkHttpAdapter as BotFrameworkAdapter;
                ResourceResponse resourceResponse = null;

                await this.retryPolicy.ExecuteAsync(async () =>
                {
                    await botFrameworkAdapter.ContinueConversationAsync(
                      this.microsoftAppCredentials.MicrosoftAppId,
                      conversationReference,
                      async (turnContext, cancellationToken) =>
                      {
                          resourceResponse = await turnContext.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
                      },
                      CancellationToken.None);
                });
            }
#pragma warning disable CA1031 // Caching general exception to log exception and user Id
            catch (Exception ex)
#pragma warning restore CA1031 // Caching general exception to log exception and user Id
            {
                this.logger.LogError(ex, $"Unable to send notification to user {user.UserId}");
            }
        }
    }
}