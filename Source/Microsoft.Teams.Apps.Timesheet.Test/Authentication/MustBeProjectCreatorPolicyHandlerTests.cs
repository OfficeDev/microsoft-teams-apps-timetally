// <copyright file="MustBeProjectCreatorPolicyHandlerTests.cs" company="Microsoft Corporation">
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The class lists unit tests cases related to MustBeProjectCreatorPolicyHandler authorization.
    /// </summary>
    [TestClass]
    public class MustBeProjectCreatorPolicyHandlerTests
    {
        /// <summary>
        /// Holds the instance of <see cref="MustBeProjectCreatorPolicyHandler"/>.
        /// </summary>
        private MustBeProjectCreatorPolicyHandler mustBeProjectCreatorPolicyHandler;

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
            this.memoryCache = new FakeMemoryCache();
            this.botOptions = new Mock<IOptions<BotSettings>>();
            this.httpAccessors = this.GetFakeHttpAccessorsForMustBeProjectCreatorPolicy();
            this.mustBeProjectCreatorPolicyHandler = new MustBeProjectCreatorPolicyHandler(this.memoryCache, this.repositoryAccessors.Object, this.botOptions.Object, this.httpAccessors);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeProjectCreatorPolicyHandler"/> policy succeed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_UserIsProjectCreator_Succeed()
        {
            // ARRANGE
            var projects = new List<Project>()
            {
                new Project
                {
                    Id = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                    Title = "TimesheetEntity App",
                    ClientName = "Microsoft",
                    BillableHours = 200,
                    NonBillableHours = 200,
                    StartDate = new DateTime(DateTime.UtcNow.Year, 1, 2),
                    EndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 28),
                    Members = new List<Member>
                    {
                        new Member
                        {
                            Id = Guid.Parse("d3d964ae-2979-4dac-b1e0-6c1b936c2640"),
                            ProjectId = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                            UserId = Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"),
                            IsBillable = true,
                            IsRemoved = false,
                        },
                    },
                    Tasks = new List<TaskEntity>
                    {
                        new TaskEntity
                        {
                            Id = Guid.Parse("2dcf17b4-9bc7-488a-a59c-b0d12b14782d"),
                            ProjectId = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                            IsRemoved = false,
                            Title = "Development",
                        },
                    },
                    CreatedBy = Guid.Parse("08310120-ff64-45a4-b67a-6f2f19fba937"),
                    CreatedOn = DateTime.Now,
                },
            };
            var projectRepository = new Mock<IProjectRepository>();
            projectRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(Task.FromResult(projects.AsEnumerable()));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerProjectValidationCacheDurationInHours = 1 });

            var authorizationContext = this.GetFakeAuthorizationHandlerContextForMustBeProjectCreatorPolicy();
            await this.mustBeProjectCreatorPolicyHandler.HandleAsync(authorizationContext);

            Assert.IsTrue(authorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeProjectCreatorPolicyHandler"/> policy failed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_UserIsNotProjectCreator_Failed()
        {
            var projectRepository = new Mock<IProjectRepository>();
            projectRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(Task.FromResult(Enumerable.Empty<Project>()));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerProjectValidationCacheDurationInHours = 1 });

            var authorizationContext = this.GetFakeAuthorizationHandlerContextForMustBeProjectCreatorPolicy();
            await this.mustBeProjectCreatorPolicyHandler.HandleAsync(authorizationContext);

            Assert.IsFalse(authorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Get fake authorization handler context for MustBeProjectCreatorPolicy.
        /// </summary>
        /// <returns>Authorization handler context for MustBeProjectCreatorPolicy.</returns>
        private AuthorizationHandlerContext GetFakeAuthorizationHandlerContextForMustBeProjectCreatorPolicy()
        {
            var mustBeProjectMemberPolicyRequirement = new[] { new MustBeProjectCreatorRequirement() };

            var context = this.GetFakeHttpContext();
            var filters = new List<IFilterMetadata>();
            var resource = new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), filters);

            context.Request.RouteValues.Add("projectId", "1a1cce71-2833-4345-86e2-e9047f73e6af");

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

        /// <summary>
        /// Gets fake HTTP accessors for must be project creator policy.
        /// </summary>
        /// <returns>Fake http context accessors.</returns>
        private HttpContextAccessor GetFakeHttpAccessorsForMustBeProjectCreatorPolicy()
        {
            var httpAccessors = new HttpContextAccessor();
            var httpContext = this.GetFakeHttpContext();
            httpContext.Request.RouteValues.Add("projectId", "1a1cce71-2833-4345-86e2-e9047f73e6af");
            httpAccessors.HttpContext = httpContext;
            return httpAccessors;
        }
    }
}
