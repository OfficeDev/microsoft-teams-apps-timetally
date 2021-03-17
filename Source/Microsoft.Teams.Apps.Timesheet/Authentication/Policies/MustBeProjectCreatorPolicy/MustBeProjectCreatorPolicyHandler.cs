// <copyright file="MustBeProjectCreatorPolicyHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Authentication
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// This authorization handler is created to handle manager access policy.
    /// The class implements AuthorizationHandler for handling MustBeProjectCreatorPolicyRequirement authorization.
    /// </summary>
    public class MustBeProjectCreatorPolicyHandler : IAuthorizationHandler
    {
        /// <summary>
        /// A set of key/value application configuration properties for caching settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Cache for storing authorization result.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Instance of repository accessors for fetching valid projects.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessor;

        /// <summary>
        /// The instance of HTTP context accessors.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MustBeProjectCreatorPolicyHandler"/> class.
        /// </summary>
        /// <param name="memoryCache">Memory cache instance for caching authorization result.</param>
        /// <param name="repositoryAccessor">Instance of repository accessors for fetching valid projects.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for caching settings.</param>
        /// <param name="httpContextAccessor">The instance of HTTP context accessors.</param>
        public MustBeProjectCreatorPolicyHandler(
           IMemoryCache memoryCache,
           IRepositoryAccessors repositoryAccessor,
           IOptions<BotSettings> botOptions,
           IHttpContextAccessor httpContextAccessor)
        {
            this.memoryCache = memoryCache;
            this.repositoryAccessor = repositoryAccessor;
            this.botOptions = botOptions;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// This method handles the authorization requirement.
        /// </summary>
        /// <param name="context">AuthorizationHandlerContext instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            var oidClaim = context.User.Claims.FirstOrDefault(p => oidClaimType == p.Type);

            foreach (var requirement in context.Requirements)
            {
                if (requirement is MustBeProjectCreatorRequirement)
                {
                    if (context.Resource is AuthorizationFilterContext authorizationFilterContext)
                    {
                        var isValuePresent = authorizationFilterContext.HttpContext.Request.RouteValues.TryGetValue("projectId", out object projectIdFromRoute);

                        if (isValuePresent)
                        {
                            var projectId = Guid.Parse(projectIdFromRoute.ToString());
                            if (await this.ValidateIfManagerCreatedProjectAsync(Guid.Parse(oidClaim.Value), projectId))
                            {
                                context.Succeed(requirement);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if logged-in manager has created the requested project.
        /// </summary>
        /// <param name="userAadObjectId">The user's Azure Active Directory object Id.</param>
        /// <param name="projectId">Unique Id of project for fetching details.</param>
        /// <returns>The flag indicates that the user has created project.</returns>
        private async Task<bool> ValidateIfManagerCreatedProjectAsync(Guid userAadObjectId, Guid projectId)
        {
            var isEntryAvailableInCache = this.memoryCache.TryGetValue(this.GetCacheKey(userAadObjectId.ToString(), projectId.ToString()), out bool isProjectCreatedByManager);

            if (!isEntryAvailableInCache)
            {
                var projects = await this.repositoryAccessor.ProjectRepository.FindAsync(project => project.Id == projectId && project.CreatedBy == userAadObjectId);
                isProjectCreatedByManager = projects.Any();

                this.memoryCache.Set(this.GetCacheKey(userAadObjectId.ToString(), projectId.ToString()), isProjectCreatedByManager, TimeSpan.FromHours(this.botOptions.Value.ManagerProjectValidationCacheDurationInHours));
            }

            return isProjectCreatedByManager;
        }

        /// <summary>
        /// Generate key by user object Id and project Id.
        /// </summary>
        /// <param name="userAadObjectId">The user's Azure Active Directory object Id.</param>
        /// <param name="projectId">Project Id for which details are requested.</param>
        /// <returns>Generated key.</returns>
        private string GetCacheKey(string userAadObjectId, string projectId)
        {
            return $"manager_{userAadObjectId}$project_{projectId}";
        }
    }
}
