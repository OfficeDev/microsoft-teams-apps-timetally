// <copyright file="IManagerDashboardMapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Interface for handling operations related to manager dashboard models mapping.
    /// </summary>
    public interface IManagerDashboardMapper
    {
        /// <summary>
        /// Gets dashboard request view model to be sent as API response.
        /// </summary>
        /// <param name="timesheetsCollection">Collection of list of timesheet entity model.</param>
        /// <returns>Returns a dashboard request view entity model.</returns>
        IEnumerable<DashboardRequestDTO> MapForViewModel(IEnumerable<IEnumerable<TimesheetEntity>> timesheetsCollection);

        /// <summary>
        /// Gets dashboard project view model to be sent as API response.
        /// </summary>
        /// <param name="project">The project entity model.</param>
        /// <param name="timesheets">List of timesheet entity model.</param>
        /// <returns>Returns a dashboard project view entity model.</returns>
        DashboardProjectDTO MapForDashboardProjectViewModel(Project project, IEnumerable<TimesheetEntity> timesheets);
    }
}