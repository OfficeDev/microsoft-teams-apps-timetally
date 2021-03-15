// <copyright file="UserControllerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Controllers;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// User controller tests contains all the test cases for the graph operations.
    /// </summary>
    [TestClass]
    public class UserControllerTests
    {
        private readonly List<User> users = new List<User>
        {
            new User
            {
                Id = "99051013-15d3-4831-a301-ded45bf3d12a",
            },
        };

        private readonly List<SubmittedRequestDTO> expectedSubmittedRequestDTO = new List<SubmittedRequestDTO>
        {
            new SubmittedRequestDTO
            {
                Status = (int)TimesheetStatus.Submitted,
                UserId = Guid.Parse("1a1a285f-7b97-45a8-82c3-58562b69a1ce"),
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 25),
                ProjectTitles = new List<string>
                {
                    "Project 1",
                },
                SubmittedTimesheetIds = new List<Guid>
                {
                    Guid.Parse("0a0a285f-7b97-45a8-82c3-58562b69a1ce"),
                },
                TotalHours = 10,
            },
        };

        /// <summary>
        /// Holds the instance telemetryClient.
        /// </summary>
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Holds the instance of user controller.
        /// </summary>
        private UsersController userController;

        /// <summary>
        /// Mocked the instance of user graph service.
        /// </summary>
        private Mock<IUsersService> userGraphService;

        /// <summary>
        /// The mocked instance of user helper.
        /// </summary>
        private Mock<IUserHelper> userHelper;

        /// <summary>
        /// Mocked instance of logger.
        /// </summary>
        private Mock<ILogger<UsersController>> logger;

        /// <summary>
        /// The mocked instance of timesheet helper.
        /// </summary>
        private Mock<ITimesheetHelper> timesheetHelper;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.logger = new Mock<ILogger<UsersController>>();
            this.telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            this.userGraphService = new Mock<IUsersService>();
            this.timesheetHelper = new Mock<ITimesheetHelper>();
            this.userHelper = new Mock<IUserHelper>();
            this.userController = new UsersController(this.logger.Object, this.userGraphService.Object, this.telemetryClient, this.timesheetHelper.Object, this.userHelper.Object);
            var httpContext = FakeHttpContext.MakeFakeContext();
            this.userController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
        }

        /// <summary>
        /// Test whether we can get reportee with random string.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CanGetReporteeAsync()
        {
            // ARRANGE
            this.userGraphService
                .Setup(graphService => graphService.GetMyReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<User>() as IEnumerable<User>));

            // ACT
            var result = (ObjectResult)await this.userController.GetMyReporteesAsync("random");

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status200OK);
        }

        /// <summary>
        /// Test whether we can get manager.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CanGetManagerAsync()
        {
            // ARRANGE
            this.userGraphService
                .Setup(graphService => graphService.GetManagerAsync())
                .Returns(Task.FromResult(new DirectoryObject()));

            // ACT
            var result = (ObjectResult)await this.userController.GetManagerAsync();

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status200OK);
        }

        /// <summary>
        /// Test whether unauthorized status is return when user not report to logged in manager while fetching timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetTimesheetRequestsByStatus_WhenUserNotReportToLoggedInManager_ShouldReturnUnauthorizedStatus()
        {
            // ARRANGE
            this.userGraphService
                .Setup(service => service.GetMyReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(this.users.AsEnumerable()));

            var reporteeId = Guid.NewGuid();

            // ACT
            var result = (ObjectResult)await this.userController.GetTimesheetsByStatusAsync(reporteeId, (int)TimesheetStatus.Submitted);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status401Unauthorized, result.StatusCode);
        }

        /// <summary>
        /// Test whether bad request status is return with invalid timesheet status while fetching timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetTimesheetRequestsByStatus_WithInvalidTimesheetStatus_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var reporteeId = Guid.NewGuid();
            var invalidTimesheetStatus = 8;

            // ACT
            var result = (ObjectResult)await this.userController.GetTimesheetsByStatusAsync(reporteeId, invalidTimesheetStatus);

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Test whether OK status is return with valid parameters while fetching timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetTimesheetRequestsByStatus_WithValidParams_ShouldReturnOKStatus()
        {
            // ARRANGE
            this.userGraphService
                .Setup(service => service.GetMyReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(this.users.AsEnumerable()));
            this.timesheetHelper
                .Setup(helper => helper.GetTimesheetsByStatus(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()))
                .Returns(this.expectedSubmittedRequestDTO.AsEnumerable());
            var reporteeId = Guid.Parse("99051013-15d3-4831-a301-ded45bf3d12a");

            // ACT
            var result = (ObjectResult)await this.userController.GetTimesheetsByStatusAsync(reporteeId, (int)TimesheetStatus.Approved);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }

        /// <summary>
        /// Test whether OK status is return with valid parameters while fetching user timesheet overview.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetUserTimesheetsOverviewAsync_WithValidParams_ShouldReturnOKStatus()
        {
            // ARRANGE
            this.userHelper
                .Setup(helper => helper.GetAllReporteesAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(this.users.AsEnumerable()));
            this.timesheetHelper
                .Setup(helper => helper.GetTimesheetsByStatus(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()))
                .Returns(this.expectedSubmittedRequestDTO.AsEnumerable());
            var reporteeId = Guid.Parse("99051013-15d3-4831-a301-ded45bf3d12a");
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 6);

            // ACT
            var result = (ObjectResult)await this.userController.GetUserTimesheetsOverviewAsync(startDate, endDate, reporteeId);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}