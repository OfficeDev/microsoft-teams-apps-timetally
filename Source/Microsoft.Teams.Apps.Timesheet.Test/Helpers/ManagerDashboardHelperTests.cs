// <copyright file="ManagerDashboardHelperTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Manager dashboard helper tests contains all the test cases for helping methods.
    /// </summary>
    [TestClass]
    public class ManagerDashboardHelperTests
    {
        /// <summary>
        /// Approved timesheet test list.
        /// </summary>
        private readonly IEnumerable<TimesheetEntity> approvedTimesheets = new List<TimesheetEntity>
        {
            new TimesheetEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                Status = (int)TimesheetStatus.Approved,
                Hours = 5,
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                Task = new TaskEntity
                {
                    ProjectId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                    Project = new Project
                    {
                        Title = "Project",
                    },
                },
            },
            new TimesheetEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                Status = (int)TimesheetStatus.Approved,
                Hours = 5,
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                Task = new TaskEntity
                {
                    ProjectId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                    Project = new Project
                    {
                        Title = "Project",
                    },
                },
            },
        };

        /// <summary>
        /// Holds the instance of manager dashboard helper.
        /// </summary>
        private ManagerDashboardHelper managerDashboardHelper;

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
        /// Mocked instance of graph service.
        /// </summary>
        private Mock<IUsersService> userGraphService;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.timesheetContext = new Mock<TimesheetContext>();
            this.timesheetRepository = new Mock<ITimesheetRepository>();
            this.projectRepository = new Mock<IProjectRepository>();
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.userGraphService = new Mock<IUsersService>();
            this.managerDashboardHelper = new ManagerDashboardHelper(this.timesheetContext.Object, this.repositoryAccessors.Object, this.userGraphService.Object, new ManagerDashboardMapper());
        }

        /// <summary>
        /// Test whether we can get dashboard requests with valid data.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardRequests_WithValidParams_ShouldReturnValidData()
        {
            // ARRANGE
            var savedTimesheets = new List<TimesheetEntity>
            {
                new TimesheetEntity
                {
                    Id = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    Status = (int)TimesheetStatus.Saved,
                    Hours = 5,
                    TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                    Task = new TaskEntity
                    {
                        ProjectId = Guid.NewGuid(),
                        Project = new Project
                        {
                            Title = "Project",
                        },
                    },
                },
                new TimesheetEntity
                {
                    Id = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    Status = (int)TimesheetStatus.Saved,
                    Hours = 5,
                    TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                    Task = new TaskEntity
                    {
                        ProjectId = Guid.NewGuid(),
                        Project = new Project
                        {
                            Title = "Project",
                        },
                    },
                },
                new TimesheetEntity
                {
                    Id = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    Status = (int)TimesheetStatus.Saved,
                    Hours = 5,
                    TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 6),
                    Task = new TaskEntity
                    {
                        ProjectId = Guid.NewGuid(),
                        Project = new Project
                        {
                            Title = "Project",
                        },
                    },
                },
                new TimesheetEntity
                {
                    Id = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    Status = (int)TimesheetStatus.Saved,
                    Hours = 5,
                    TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 8),
                    Task = new TaskEntity
                    {
                        ProjectId = Guid.NewGuid(),
                        Project = new Project
                        {
                            Title = "Project",
                        },
                    },
                },
            };
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.timesheetRepository
                 .Setup(timesheetRepo => timesheetRepo.GetTimesheetsByManagerId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()))
                 .Returns(savedTimesheets
                    .AsEnumerable()
                    .GroupBy(x => x.UserId)
                    .ToDictionary(x => x.Key, x => x.AsEnumerable()));
            this.userGraphService
                .Setup(graphService => graphService.GetUsersAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(new List<User>
                {
                    new User
                    {
                        Id = "3fd7af65-67df-43cb-baa0-30917e133d94",
                        DisplayName = "Random",
                    },
                }.AsEnumerable()
                .ToDictionary(user => Guid.Parse(user.Id), user => user)));

            var managerId = Guid.NewGuid();

            // ACT
            var dashboardRequestsDTO = await this.managerDashboardHelper.GetDashboardRequestsAsync(managerId);

            // ASSERT
            Assert.AreEqual(1, dashboardRequestsDTO.Count());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetsByManagerId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Test whether empty list is return when timesheet not found while fetching dashboard request.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardRequests_WhenTimesheetsNotFound_ShouldReturnEmptyList()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);

            this.timesheetRepository
                    .Setup(timesheetRepo => timesheetRepo.GetTimesheetsByManagerId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()))
                    .Returns(new List<TimesheetEntity>()
                    .AsEnumerable()
                    .GroupBy(x => x.UserId)
                    .ToDictionary(x => x.Key, x => x.AsEnumerable()));
            this.userGraphService
                .Setup(graphService => graphService.GetUsersAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(new List<User>
                {
                    new User
                    {
                        Id = "2fd7af65-67df-43cb-baa0-30917e133d94",
                        DisplayName = "Random",
                    },
                }.AsEnumerable()
                .ToDictionary(user => Guid.Parse(user.Id), user => user)));

            var managerId = Guid.NewGuid();

            // ACT
            var dashboardRequestDTO = await this.managerDashboardHelper.GetDashboardRequestsAsync(managerId);

            // ASSERT
            Assert.IsTrue(dashboardRequestDTO.IsNullOrEmpty());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetsByManagerId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Test whether we can get dashboard projects with valid parameters.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardProjects_WithValidParams_ShouldReturnValidData()
        {
            // ARRANGE
            var project = new Project
            {
                BillableHours = 50,
                NonBillableHours = 10,
                ClientName = "Samuel,",
                CreatedBy = Guid.Parse("2626664f-390a-489c-a57c-4d08ee843950"),
                CreatedOn = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day),
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month, 28),
                Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                Title = "Project 1",
                Members = new List<Member>
                {
                    new Member
                    {
                        Id = Guid.NewGuid(),
                        IsBillable = true,
                        IsRemoved = false,
                        ProjectId = Guid.NewGuid(),
                        UserId = Guid.Parse("1ce072c1-1b87-4912-bb60-307698e6874e"),
                    },
                },
                Tasks = new List<TaskEntity>
                {
                    new TaskEntity
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = Guid.NewGuid(),
                        Title = "TaskEntity",
                    },
                },
            };
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectIds(It.IsAny<IEnumerable<Guid>>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(this.approvedTimesheets.AsEnumerable());
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetActiveProjectsAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new List<Project>
                {
                    project,
                }.AsEnumerable()));

            var managerUserObjectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var result = await this.managerDashboardHelper.GetDashboardProjectsAsync(managerUserObjectId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(TestData.ExpectedDashboardProjects.Count(), result.Count());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectIds(It.IsAny<IEnumerable<Guid>>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
            this.projectRepository.Verify(projectRepo => projectRepo.GetActiveProjectsAsync(It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether empty list is return when projects not found while fetching dashboard projects.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardProjects_WhenProjectsNotFound_ShouldReturnEmptyList()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectIds(It.IsAny<IEnumerable<Guid>>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(this.approvedTimesheets.AsEnumerable());
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetActiveProjectsAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Enumerable.Empty<Project>()));

            var managerUserObjectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day - 1);

            // ACT
            var result = await this.managerDashboardHelper.GetDashboardProjectsAsync(managerUserObjectId, startDate, endDate);

            // ASSERT
            Assert.IsTrue(result.IsNullOrEmpty());
            this.projectRepository.Verify(projectRepo => projectRepo.GetActiveProjectsAsync(It.IsAny<Guid>()), Times.Once());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectIds(It.IsAny<IEnumerable<Guid>>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never());
        }
    }
}