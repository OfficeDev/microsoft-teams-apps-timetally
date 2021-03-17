// <copyright file="MustHaveReporteesPolicyHandlerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Test.Authentication
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The class lists unit tests cases related to MustHaveReporteesPolicyHandler authorization.
    /// </summary>
    [TestClass]
    public class MustHaveReporteesPolicyHandlerTests
    {
        /// <summary>
        /// Holds the instance of <see cref="MustBeManagerPolicyHandler"/>.
        /// </summary>
        private MustBeManagerPolicyHandler mustHaveReporteesPolicyHandler;

        /// <summary>
        /// The mocked instance of user service.
        /// </summary>
        private Mock<IUsersService> userService;

        /// <summary>
        /// The instance of memory cache.
        /// </summary>
        private IMemoryCache memoryCache;

        /// <summary>
        /// The mocked instance of bot settings.
        /// </summary>
        private Mock<IOptions<BotSettings>> botOptions;

        /// <summary>
        /// Initializes all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.userService = new Mock<IUsersService>();
            this.memoryCache = new FakeMemoryCache();
            this.botOptions = new Mock<IOptions<BotSettings>>();
            this.mustHaveReporteesPolicyHandler = new MustBeManagerPolicyHandler(this.memoryCache, this.userService.Object, this.botOptions.Object);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeManagerPolicyHandler"/> policy succeed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_HasReportees_Succeed()
        {
            this.userService
                .Setup(x => x.GetMyReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(TestData.Reportees.AsEnumerable()));

            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerReporteesCacheDurationInHours = 1 });

            var authorizationContext = this.GetFakeAuthorizationHandlerContextForMustHaveReporteesPolicy();
            await this.mustHaveReporteesPolicyHandler.HandleAsync(authorizationContext);

            Assert.IsTrue(authorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeManagerPolicyHandler"/> policy failed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_NoReportees_Failed()
        {
            this.userService
                .Setup(x => x.GetMyReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(Enumerable.Empty<User>()));

            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerReporteesCacheDurationInHours = 1 });

            var authorizationContext = this.GetFakeAuthorizationHandlerContextForMustHaveReporteesPolicy();
            await this.mustHaveReporteesPolicyHandler.HandleAsync(authorizationContext);

            Assert.IsFalse(authorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Get fake authorization handler context for MustHaveReporteesPolicyHandler.
        /// </summary>
        /// <returns>Authorization handler context for MustHaveReporteesPolicyHandler.</returns>
        private AuthorizationHandlerContext GetFakeAuthorizationHandlerContextForMustHaveReporteesPolicy()
        {
            var mustHaveReporteesPolicyRequirement = new[] { new MustBeManagerPolicyRequirement() };

            var context = this.GetFakeHttpContext();
            var filters = new List<IFilterMetadata>();
            var resource = new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), filters);

            context.Request.RouteValues.Add("projectId", "1a1cce71-2833-4345-86e2-e9047f73e6af");

            return new AuthorizationHandlerContext(mustHaveReporteesPolicyRequirement, context.User, resource);
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
    }
}
