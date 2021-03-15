// <copyright file="IProjectMapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Exposes methods that manages project model mappings.
    /// </summary>
    public interface IProjectMapper
    {
        /// <summary>
        /// Gets project model to be inserted in database.
        /// </summary>
        /// <param name="projectViewModel">Project entity view model.</param>
        /// <param name="userObjectId">Azure Active Directory Id of logged-in user.</param>
        /// <returns>Returns a project entity model.</returns>
        Project MapForCreateModel(ProjectDTO projectViewModel, Guid userObjectId);

        /// <summary>
        /// Gets project model to be updated in database.
        /// </summary>
        /// <param name="projectViewModel">Project entity view model.</param>
        /// <param name="projectModel">Project entity model.</param>
        /// <returns>Returns a project entity model.</returns>
        Project MapForUpdateModel(ProjectUpdateDTO projectViewModel, Project projectModel);

        /// <summary>
        /// Gets project view model to be sent as API response.
        /// </summary>
        /// <param name="projectModel">Project entity model.</param>
        /// <returns>Returns a project view entity model.</returns>
        ProjectDTO MapForViewModel(Project projectModel);

        /// <summary>
        /// Gets project utilization view model to be sent as API response.
        /// </summary>
        /// <param name="project">The project entity model.</param>
        /// <param name="timesheets">Collection of timesheet entity model.</param>
        /// <param name="members">List of project members.</param>
        /// <returns>Returns a project utilization view entity model.</returns>
        ProjectUtilizationDTO MapForProjectUtilizationViewModel(Project project, IEnumerable<TimesheetEntity> timesheets, IEnumerable<Member> members);
    }
}