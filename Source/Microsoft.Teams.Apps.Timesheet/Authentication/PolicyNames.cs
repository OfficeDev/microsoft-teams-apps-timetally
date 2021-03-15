// <copyright file="PolicyNames.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Authentication
{
    /// <summary>
    /// This class lists the names of the custom authorization policies in the project.
    /// </summary>
    public static class PolicyNames
    {
        /// <summary>
        /// The name of the authorization policy, MustBeProjectMemberPolicy. Indicates that user must be part of any projects.
        /// </summary>
        public const string MustBeProjectMemberPolicy = "MustBeProjectMemberPolicy";

        /// <summary>
        /// The name of the authorization policy, MustBeManagerPolicy. Indicates that user must have at least 1 reportee.
        /// </summary>
        public const string MustBeManagerPolicy = "MustBeManagerPolicy";

        /// <summary>
        /// The name of the authorization policy, MustBeProjectCreatorPolicy. Indicates that user must be creator of the requested project.
        /// </summary>
        public const string MustBeProjectCreatorPolicy = "MustBeProjectCreatorPolicy";

        /// <summary>
        /// The name of the authorization policy, MustBeValidReporteePolicy. Indicates that received reportees must be valid for logged-in manager.
        /// </summary>
        public const string MustBeManagerOfReporteePolicy = "MustBeManagerOfReporteePolicy";
    }
}
