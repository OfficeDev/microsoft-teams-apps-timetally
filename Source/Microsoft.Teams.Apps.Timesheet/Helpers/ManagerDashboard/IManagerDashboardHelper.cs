// <copyright file="IManagerDashboardHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Provides helper methods for managing managers dashboard.
    /// </summary>
    public interface IManagerDashboardHelper
    {
        /// <summary>
        /// Gets timesheet which are pending for manager approval.
        /// </summary>
        /// <param name="managerObjectId">The manager Id for which request has been raised.</param>
        /// <returns>Return list of submitted timesheet.</returns>
        Task<IEnumerable<DashboardRequestDTO>> GetDashboardRequestsAsync(Guid managerObjectId);

        /// <summary>
        /// Get approved and active project details for dashboard between date range.
        /// </summary>
        /// <param name="managerUserObjectId">The manager user object Id who created a project.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of dashboard projects.</returns>
        Task<IEnumerable<DashboardProjectDTO>> GetDashboardProjectsAsync(Guid managerUserObjectId, DateTime startDate, DateTime endDate);
    }
}
