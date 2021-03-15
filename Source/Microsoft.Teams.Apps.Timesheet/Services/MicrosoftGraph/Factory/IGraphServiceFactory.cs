// <copyright file="IGraphServiceFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph
{
    /// <summary>
    /// Interface for Graph service factory.
    /// </summary>
    public interface IGraphServiceFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="IUsersService"/> implementation.
        /// </summary>
        /// <returns>Returns an implementation of <see cref="IUsersService"/>.</returns>
        public IUsersService GetUsersService();
    }
}