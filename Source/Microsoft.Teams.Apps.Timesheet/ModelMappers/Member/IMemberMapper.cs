// <copyright file="IMemberMapper.cs" company="Microsoft Corporation">
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
    /// Interface exposes methods used for member models mapping.
    /// </summary>
    public interface IMemberMapper
    {
        /// <summary>
        /// Gets members model to be inserted in database.
        /// </summary>
        /// <param name="projectId">The Id of the project in which members need to be added.</param>
        /// <param name="membersViewModel">Members entity view model.</param>
        /// <returns>Returns list of members model.</returns>
        IEnumerable<Member> MapForCreateModel(Guid projectId, IEnumerable<MemberDTO> membersViewModel);

        /// <summary>
        /// Gets members model to be updated in database.
        /// </summary>
        /// <param name="changesToApply">Updated details of members.</param>
        /// <param name="existingMembers">List of existing members.</param>
        /// <returns>Returns list of member entity model.</returns>
        IEnumerable<Member> MapForExistingMembers(IEnumerable<MemberDTO> changesToApply, IEnumerable<Member> existingMembers);

        /// <summary>
        /// Gets project members overview view model to be sent as API response.
        /// </summary>
        /// <param name="members">List of members entity model.</param>
        /// <param name="timesheets">List of timesheet entity model.</param>
        /// <returns>Returns a list of project members overview view entity model.</returns>
        IEnumerable<ProjectMemberOverviewDTO> MapForProjectMembersViewModel(IEnumerable<Member> members, IEnumerable<TimesheetEntity> timesheets);
    }
}