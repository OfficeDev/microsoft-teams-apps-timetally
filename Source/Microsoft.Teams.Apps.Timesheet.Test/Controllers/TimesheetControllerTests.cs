// <copyright file="TimesheetControllerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Controllers;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Timesheet controller tests contains all the test cases for the timesheet CRUD operations.
    /// </summary>
    [TestClass]
    public class TimesheetControllerTests
    {
        /// <summary>
        /// The project test data.
        /// </summary>
        private readonly List<Project> projects = new List<Project>()
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

        /// <summary>
        /// Timesheet test list.
        /// </summary>
        private readonly List<TimesheetEntity> timesheets = new List<TimesheetEntity>
        {
            new TimesheetEntity
            {
                // Given same date as given in user timesheet test list.
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10),
                TaskId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515540e"),
                Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                Status = (int)TimesheetStatus.None,
                Task = new TaskEntity
                {
                    ProjectId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                },
            },
            new TimesheetEntity
            {
                // Given same date as given in user timesheet test list.
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10),
                TaskId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515540e"),
                Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                Status = (int)TimesheetStatus.None,
                Task = new TaskEntity
                {
                    ProjectId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                },
            },
        };

        /// <summary>
        /// The project test data.
        /// </summary>
        private readonly List<Project> allProjectsAssignedToUser = new List<Project>()
        {
            new Project
            {
                Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                Title = "Timesheet App",
                ClientName = "Microsoft",
                BillableHours = 200,
                NonBillableHours = 200,
                StartDate = new DateTime(2021, 1, 2),
                EndDate = new DateTime(2021, 2, 10),
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

        /// <summary>
        /// User timesheet test list.
        /// </summary>
        private readonly List<UserTimesheet> userTimesheets = new List<UserTimesheet>
        {
            new UserTimesheet
            {
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 25),
                ProjectDetails = new List<ProjectDetails>
                {
                    new ProjectDetails
                    {
                        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                        EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 25),
                        Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                        Title = "Project",
                        TimesheetDetails = new List<TimesheetDetails>
                        {
                            new TimesheetDetails
                            {
                                Hours = 4,
                                ManagerComments = string.Empty,
                                Status = (int)TimesheetStatus.Submitted,
                                TaskId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515540e"),
                                TaskTitle = "Task",
                            },
                        },
                    },
                },
            },
            new UserTimesheet
            {
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 25),
                ProjectDetails = new List<ProjectDetails>
                {
                    new ProjectDetails
                    {
                        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 25),
                        EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 26),
                        Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                        Title = "Project",
                        TimesheetDetails = new List<TimesheetDetails>
                        {
                            new TimesheetDetails
                            {
                                Hours = 4,
                                ManagerComments = string.Empty,
                                Status = (int)TimesheetStatus.Submitted,
                                TaskId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515540e"),
                                TaskTitle = "Task",
                            },
                        },
                    },
                },
            },
        };

        /// <summary>
        /// Holds the instance telemetryClient.
        /// </summary>
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Holds the instance of timesheet controller.
        /// </summary>
        private TimesheetController timesheetController;

        /// <summary>
        /// The mocked instance of timesheet helper.
        /// </summary>
        private Mock<ITimesheetHelper> timesheetHelper;

        /// <summary>
        /// The mocked instance of manager dashboard helper.
        /// </summary>
        private Mock<IManagerDashboardHelper> managerDashboardHelper;

        /// <summary>
        /// Mocked instance of timesheet controller logger.
        /// </summary>
        private Mock<ILogger<TimesheetController>> timesheetControllerLogger;

        /// <summary>
        /// The mocked instance of timesheet repository.
        /// </summary>
        private Mock<ITimesheetRepository> timesheetRepository;

        /// <summary>
        /// The mocked instance of project repository.
        /// </summary>
        private Mock<IProjectRepository> projectRepository;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// The mocked instance of timesheet database context.
        /// </summary>
        private Mock<TimesheetContext> timesheetContext;

        /// <summary>
        /// The mocked instance of bot settings.
        /// </summary>
        private IOptions<BotSettings> mockBotSettings;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.timesheetControllerLogger = new Mock<ILogger<TimesheetController>>();
            this.telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            this.timesheetContext = new Mock<TimesheetContext>();
            this.timesheetRepository = new Mock<ITimesheetRepository>();
            this.projectRepository = new Mock<IProjectRepository>();
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.managerDashboardHelper = new Mock<IManagerDashboardHelper>();
            this.mockBotSettings = Options.Create(new BotSettings()
            {
                MicrosoftAppId = string.Empty,
                MicrosoftAppPassword = string.Empty,
                AppBaseUri = string.Empty,
                CardCacheDurationInHour = 12,
                TimesheetFreezeDayOfMonth = 12,
                WeeklyEffortsLimit = 44,
            });
            this.timesheetHelper = new Mock<ITimesheetHelper>();
            this.timesheetController = new TimesheetController(this.timesheetControllerLogger.Object, this.telemetryClient, this.timesheetHelper.Object, this.managerDashboardHelper.Object, this.repositoryAccessors.Object);
            var httpContext = FakeHttpContext.MakeFakeContext();
            this.timesheetController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
        }

        /// <summary>
        /// Tests whether duplicate efforts operation unsuccessful if frozen dates are provided.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DuplicateTimesheets_ProvideFrozenTargetDates_ReturnsBadRequest()
        {
            var duplicateEfforts = new DuplicateEffortsDTO
            {
                SourceDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                TargetDates = new List<DateTime>
                {
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, 6),
                },
            };

            this.timesheetHelper
                .Setup(helper => helper.GetNotYetFrozenTimesheetDates(It.IsAny<List<DateTime>>(), DateTimeOffset.Now))
                .Returns(new List<DateTime>());
            this.timesheetHelper
                .Setup(helper => helper.IsClientCurrentDateValid(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(true);

            var result = (ObjectResult)await this.timesheetController.DuplicateEffortsAsync(DateTime.UtcNow.Date, duplicateEfforts);

            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            var error = (ErrorResponse)result.Value;
            Assert.AreEqual("The timesheet can not be filled as the target dates are frozen.", error.Message);
        }

        /// <summary>
        /// Test whether bad request status is return with null model while rejecting timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task RejectTimesheets_WithNullModel_ShoudlReturnBadRequestStatus()
        {
            // ACT
            var result = (ObjectResult)await this.timesheetController.RejectTimesheetsAsync(null);

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Test whether not found status is return with invalid timesheets while rejecting timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task RejectTimesheets_WithInvalidTimesheets_ShoudlReturnNotFoundStatus()
        {
            // ARRANGE
            IEnumerable<TimesheetEntity> timesheets = null;

            this.timesheetHelper
                .Setup(helper => helper.GetSubmittedTimesheetsByIds(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(timesheets);

            // ACT
            var result = (ObjectResult)await this.timesheetController.RejectTimesheetsAsync(TestData.RequestApprovalDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        /// <summary>
        /// Test whether no content status is return with valid timesheets on successfully rejecting timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task RejectTimesheets_WithValidTimesheets_ShoudlReturnNoContentStatus()
        {
            // ARRANGE
            this.timesheetHelper
                .Setup(helper => helper.GetSubmittedTimesheetsByIds(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(TestData.SubmittedTimesheets);
            this.timesheetHelper
                .Setup(helper => helper.ApproveOrRejectTimesheetsAsync(It.IsAny<IEnumerable<TimesheetEntity>>(), It.IsAny<IEnumerable<RequestApprovalDTO>>(), It.IsAny<TimesheetStatus>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = (StatusCodeResult)await this.timesheetController.RejectTimesheetsAsync(TestData.RequestApprovalDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);
        }

        /// <summary>
        /// Test whether bad request status is return with null model while approving timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ApproveTimesheets_WithNullModel_ShoudlReturnBadRequestStatus()
        {
            // ACT
            var result = (ObjectResult)await this.timesheetController.ApproveTimesheetsAsync(null);

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Test whether not found status is return with invalid timesheets while approving timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task Approve_WithInvalidTimesheets_ShoudlReturnNotFoundStatus()
        {
            // ARRANGE
            IEnumerable<TimesheetEntity> timesheets = null;

            this.timesheetHelper
                .Setup(helper => helper.GetSubmittedTimesheetsByIds(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(timesheets);

            // ACT
            var result = (ObjectResult)await this.timesheetController.ApproveTimesheetsAsync(TestData.RequestApprovalDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        /// <summary>
        /// Test whether no content status is return with valid timesheets on successfully approving timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ApproveTimesheets_WithValidTimesheets_ShoudlReturnNoContentStatus()
        {
            // ARRANGE
            this.timesheetHelper
                .Setup(helper => helper.GetSubmittedTimesheetsByIds(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(TestData.SubmittedTimesheets);
            this.timesheetHelper
                .Setup(helper => helper.ApproveOrRejectTimesheetsAsync(It.IsAny<IEnumerable<TimesheetEntity>>(), It.IsAny<IEnumerable<RequestApprovalDTO>>(), It.IsAny<TimesheetStatus>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = (StatusCodeResult)await this.timesheetController.ApproveTimesheetsAsync(TestData.RequestApprovalDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);
        }

        /// <summary>
        /// Test whether OK status is return when requests found for logged-in manager's reportee while fetching dashboard requests.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardRequestsAsync_WhenRequestFound_ShoudlReturnOKStatus()
        {
            // Arrange
            this.managerDashboardHelper
                .Setup(helper => helper.GetDashboardRequestsAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.DashboardRequestDTOs.AsEnumerable()));

            // ACT
            var result = (ObjectResult)await this.timesheetController.GetDashboardRequestsAsync();

            // ASSERT
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}