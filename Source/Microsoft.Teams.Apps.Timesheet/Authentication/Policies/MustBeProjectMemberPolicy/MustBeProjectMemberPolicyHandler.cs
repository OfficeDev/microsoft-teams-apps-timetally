// <copyright file="MustBeProjectMemberPolicyHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Authentication
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// This authorization handler is created to check whether logged-in user is a part of any active projects.
    /// The class implements AuthorizationHandler for handling MustBeProjectMemberRequirement authorization.
    /// </summary>
    public class MustBeProjectMemberPolicyHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Cache for storing authorization result.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Holds instance of repository accessors to access repositories.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessors;

        /// <summary>
        /// A set of key/value application configuration properties for Activity settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MustBeProjectMemberPolicyHandler"/> class.
        /// </summary>
        /// <param name="memoryCache">Memory cache instance for caching authorization result.</param>
        /// <param name="repositoryAccessors">The instance of repository accessors.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for activity handler.</param>
        public MustBeProjectMemberPolicyHandler(
           IMemoryCache memoryCache,
           IRepositoryAccessors repositoryAccessors,
           IOptions<BotSettings> botOptions)
        {
            this.memoryCache = memoryCache;
            this.repositoryAccessors = repositoryAccessors;
            this.botOptions = botOptions;
        }

        /// <inheritdoc />
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            var oidClaim = context.User.Claims.FirstOrDefault(p => oidClaimType.Equals(p.Type, StringComparison.OrdinalIgnoreCase));

            foreach (var requirement in context.Requirements)
            {
                if (requirement is MustBeProjectMemberPolicyRequirement)
                {
                    if (await this.ValidateUserIsPartOfProjectsAsync(Guid.Parse(oidClaim.Value)))
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if logged-in user is part of any projects.
        /// </summary>
        /// <param name="userAadObjectId">The user's Azure Active Directory object id.</param>
        /// <returns>The flag indicates that the user is a part of certain projects or not.</returns>
        private async Task<bool> ValidateUserIsPartOfProjectsAsync(Guid userAadObjectId)
        {
            var isCacheEntryExists = this.memoryCache.TryGetValue(userAadObjectId.ToString(), out bool isUserPartOfProjects);

            if (!isCacheEntryExists)
            {
                var userProjects = await this.repositoryAccessors.MemberRepository.FindAsync(x => x.UserId == userAadObjectId);

                isUserPartOfProjects = userProjects.Any();

                this.memoryCache.Set(userAadObjectId.ToString(), isUserPartOfProjects, TimeSpan.FromHours(this.botOptions.Value.UserPartOfProjectsCacheDurationInHour));
            }

            return isUserPartOfProjects;
        }
    }
}
