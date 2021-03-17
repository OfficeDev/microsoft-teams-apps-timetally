// <copyright file="MustBeReporteeManagerPolicyRequirement.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Authentication
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// This authorization class implements the marker interface
    /// <see cref="IAuthorizationRequirement"/> to check if user meets project member specific requirements
    /// for accessing resources.
    /// </summary>
    public class MustBeReporteeManagerPolicyRequirement : IAuthorizationRequirement
    {
    }
}
