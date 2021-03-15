// <copyright file="CommonMicrosoftAppCredentials.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Services.CommonBot
{
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A common Microsoft app credentials object.
    /// </summary>
    public class CommonMicrosoftAppCredentials : MicrosoftAppCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonMicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="botOptions">The bot options.</param>
        public CommonMicrosoftAppCredentials(IOptions<BotOptions> botOptions)
            : base(appId: botOptions?.Value?.MicrosoftAppId, password: botOptions?.Value?.MicrosoftAppPassword)
        {
        }
    }
}
