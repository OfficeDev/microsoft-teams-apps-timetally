// <copyright file="MustBeReporteeManagerPolicyHandlerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Test.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The class lists unit tests cases related to MustBeValidReporteePolicyHandler authorization.
    /// </summary>
    [TestClass]
    public class MustBeReporteeManagerPolicyHandlerTests
    {
        /// <summary>
        /// Holds the instance of <see cref="MustBeReporteeManagerPolicyHandler"/>.
        /// </summary>
        private MustBeReporteeManagerPolicyHandler mustBeValidReporteePolicyHandler;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// The mocked instance of bot settings.
        /// </summary>
        private Mock<IOptions<BotSettings>> botOptions;

        /// <summary>
        /// The mocked instance of user helper.
        /// </summary>
        private Mock<IUserHelper> userHelper;

        /// <summary>
        /// The mocked instance of HTTP accessors.
        /// </summary>
        private IHttpContextAccessor httpAccessors;

        /// <summary>
        /// Initializes all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.botOptions = new Mock<IOptions<BotSettings>>();
            this.userHelper = new Mock<IUserHelper>();

            this.httpAccessors = this.GetFakeHttpAccessorsForMustBeValidReporteePolicy();
            this.mustBeValidReporteePolicyHandler = new MustBeReporteeManagerPolicyHandler(this.userHelper.Object, this.httpAccessors);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeReporteeManagerPolicyHandler"/> policy has succeeded.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_ValidReportee_Succeed()
        {
            // ARRANGE
            var users = new List<User>
            {
                new User
                {
                    Id = "99051013-15d3-4831-a301-ded45bf3d12a",
                },
            };
            var memberRepository = new Mock<IMemberRepository>();
            this.userHelper
                .Setup(x => x.GetAllReporteesAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(users.AsEnumerable()));

            this.repositoryAccessors.Setup(x => x.MemberRepository).Returns(memberRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerReporteesCacheDurationInHours = 1 });

            var fakeAuthorizationContext = this.GetFakeAuthorizationHandlerContextForMustBeReporteeManagerPolicy();

            // ACT
            await this.mustBeValidReporteePolicyHandler.HandleAsync(fakeAuthorizationContext);

            // ASSERT
            Assert.IsTrue(fakeAuthorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeReporteeManagerPolicyHandler"/> policy has succeeded.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_InvalidReportee_Failed()
        {
            // ARRANGE
            var memberRepository = new Mock<IMemberRepository>();
            this.userHelper
                .Setup(x => x.GetAllReporteesAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Enumerable.Empty<User>()));

            this.repositoryAccessors.Setup(x => x.MemberRepository).Returns(memberRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerReporteesCacheDurationInHours = 1 });

            var fakeAuthorizationContext = this.GetFakeAuthorizationHandlerContextForMustBeReporteeManagerPolicy();

            // ACT
            await this.mustBeValidReporteePolicyHandler.HandleAsync(fakeAuthorizationContext);

            // ASSERT
            Assert.IsFalse(fakeAuthorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Gets fake HTTP context.
        /// </summary>
        /// <returns>Fake HTTP context.</returns>
        private DefaultHttpContext GetFakeHttpContext()
        {
            var userAadObjectId = "1a1cce71-2833-4345-86e2-e9047f73e6af";

            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", userAadObjectId.ToString()),
                })),
            };

            context.Request.Headers["Authorization"] = "fake_token";

            return context;
        }

        /// <summary>
        /// Get fake authorization handler context for MustBeValidReporteePolicy.
        /// </summary>
        /// <returns>Authorization handler context for MustBeValidReporteePolicy.</returns>
        private AuthorizationHandlerContext GetFakeAuthorizationHandlerContextForMustBeReporteeManagerPolicy()
        {
            var mustBeReporteeManagerPolicyRequirement = new[] { new MustBeReporteeManagerPolicyRequirement() };

            var context = this.GetFakeHttpContext();
            var filters = new List<IFilterMetadata>();
            var resource = new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), filters);

            context.Request.RouteValues.Add("reporteeId", "99051013-15d3-4831-a301-ded45bf3d12a");

            return new AuthorizationHandlerContext(mustBeReporteeManagerPolicyRequirement, context.User, resource);
        }

        /// <summary>
        /// Gets fake HTTP accessors for must be valid reportee policy.
        /// </summary>
        /// <returns>Fake HTTP context accessors.</returns>
        private HttpContextAccessor GetFakeHttpAccessorsForMustBeValidReporteePolicy()
        {
            var httpAccessors = new HttpContextAccessor();
            var httpContext = this.GetFakeHttpContext();
            httpContext.Request.RouteValues.Add("reporteeId", "99051013-15d3-4831-a301-ded45bf3d12a");
            httpAccessors.HttpContext = httpContext;
            return httpAccessors;
        }
    }
}
