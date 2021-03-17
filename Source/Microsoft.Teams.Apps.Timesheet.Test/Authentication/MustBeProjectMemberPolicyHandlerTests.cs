// <copyright file="MustBeProjectMemberPolicyHandlerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Test.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The class lists unit tests cases related to MustBeProjectMemberPolicyHandler authorization.
    /// </summary>
    [TestClass]
    public class MustBeProjectMemberPolicyHandlerTests
    {
        /// <summary>
        /// Holds the instance of <see cref="MustBeProjectMemberPolicyHandler"/>.
        /// </summary>
        private MustBeProjectMemberPolicyHandler mustBeProjectMemberPolicyHandler;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

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
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.memoryCache = new FakeMemoryCache();
            this.botOptions = new Mock<IOptions<BotSettings>>();
            this.mustBeProjectMemberPolicyHandler = new MustBeProjectMemberPolicyHandler(this.memoryCache, this.repositoryAccessors.Object, this.botOptions.Object);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeProjectMemberPolicyHandler"/> policy has succeeded.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_UserIsPartOfProjects_Succeed()
        {
            var memberRepository = new Mock<IMemberRepository>();
            memberRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
                .Returns(Task.FromResult(TestData.Members.AsEnumerable()));

            this.repositoryAccessors.Setup(x => x.MemberRepository).Returns(memberRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { UserPartOfProjectsCacheDurationInHour = 1 });

            var fakeAuthorizationContext = this.GetFakeAuthorizationHandlerContext();
            await this.mustBeProjectMemberPolicyHandler.HandleAsync(fakeAuthorizationContext);

            Assert.IsTrue(fakeAuthorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeProjectMemberPolicyHandler"/> policy has failed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_UserIsNotPartOfProjects_Failed()
        {
            var memberRepository = new Mock<IMemberRepository>();
            memberRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
                .Returns(Task.FromResult(Enumerable.Empty<Member>()));

            this.repositoryAccessors.Setup(x => x.MemberRepository).Returns(memberRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { UserPartOfProjectsCacheDurationInHour = 1 });

            var fakeAuthorizationContext = this.GetFakeAuthorizationHandlerContext();
            await this.mustBeProjectMemberPolicyHandler.HandleAsync(fakeAuthorizationContext);

            Assert.IsFalse(fakeAuthorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Get fake authorization handler context.
        /// </summary>
        /// <returns>Authorization handler context.</returns>
        private AuthorizationHandlerContext GetFakeAuthorizationHandlerContext()
        {
            var mustBeProjectMemberPolicyRequirement = new[] { new MustBeProjectMemberPolicyRequirement() };

            var context = this.GetFakeHttpContext();
            var filters = new List<IFilterMetadata>();
            var resource = new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), filters);

            context.Request.RouteValues.Add("reporteeId", "99051013-15d3-4831-a301-ded45bf3d12a");

            return new AuthorizationHandlerContext(mustBeProjectMemberPolicyRequirement, context.User, resource);
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
