// <copyright file="GraphServiceFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph
{
    using System;
    using Microsoft.Graph;

    /// <summary>
    /// Graph service factory.
    /// </summary>
    public class GraphServiceFactory : IGraphServiceFactory
    {
        private readonly IGraphServiceClient serviceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphServiceFactory"/> class.
        /// </summary>
        /// <param name="serviceClient">Microsoft Graph service client.</param>
        public GraphServiceFactory(
            IGraphServiceClient serviceClient)
        {
            this.serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
        }

        /// <summary>
        /// Creates an instance of <see cref="IUsersService"/> implementation.
        /// </summary>
        /// <returns>Returns an implementation of <see cref="IUsersService"/>.</returns>
        public IUsersService GetUsersService()
        {
            return new UsersService(this.serviceClient);
        }
    }
}