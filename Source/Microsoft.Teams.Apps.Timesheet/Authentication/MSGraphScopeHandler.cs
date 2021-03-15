// <copyright file="MSGraphScopeHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Authentication
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Identity.Web;

    /// <summary>
    /// This class is an authorization handler, which handles the authorization requirement.
    /// </summary>
    public class MSGraphScopeHandler : AuthorizationHandler<MSGraphScopeRequirement>
    {
        /// <summary>
        /// Scope claim type.
        /// </summary>
        private readonly string claimTypeScp = "scp";

        /// <summary>
        /// Token acquisition instance to fetch authentication token on-behalf of user.
        /// </summary>
        private readonly ITokenAcquisition tokenAcquisition;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSGraphScopeHandler"/> class.
        /// </summary>
        /// <param name="tokenAcquisition">MSAL.NET token acquisition service.</param>
        public MSGraphScopeHandler(ITokenAcquisition tokenAcquisition)
        {
            this.tokenAcquisition = tokenAcquisition;
        }

        /// <summary>
        /// This method handles the authorization requirement.
        /// </summary>
        /// <param name="context">AuthorizationHandlerContext instance.</param>
        /// <param name="requirement">IAuthorizationRequirement instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MSGraphScopeRequirement requirement)
        {
            requirement = requirement ?? throw new ArgumentNullException(nameof(requirement), "Requirement instance cannot be null");
            context = context ?? throw new ArgumentNullException(nameof(context), "Context instance cannot be null");

            var hasScope = await this.HasScopesAsync(requirement.Scopes);
            if (hasScope)
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Check whether the access token has input scopes.
        /// This is where we should check if user has valid graph access.
        /// </summary>
        /// <param name="scopes">Microsoft Graph scopes.</param>
        /// <returns>Indicate if access token has scope.</returns>
        private async Task<bool> HasScopesAsync(string[] scopes)
        {
            var accessToken = await this.tokenAcquisition.GetAccessTokenForUserAsync(new[] { Timesheet.Constants.UserReadAll });
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;
            var claimValue = securityToken.Claims
                .First(claim => claim.Type.Equals(this.claimTypeScp.ToString(), StringComparison.CurrentCultureIgnoreCase)).Value;
            var intersectScopes = claimValue.ToUpperInvariant().Split(' ').Intersect(scopes.Select(scp => scp.ToUpperInvariant())).ToArray();
            return scopes.Length == intersectScopes.Length;
        }
    }
}
