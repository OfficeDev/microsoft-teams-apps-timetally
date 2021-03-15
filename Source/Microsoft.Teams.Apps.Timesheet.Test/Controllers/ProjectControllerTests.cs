// <copyright file="ProjectControllerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Controllers;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Project controller tests contains all the test cases for the project CRUD operations.
    /// </summary>
    [TestClass]
    public class ProjectControllerTests
    {
        /// <summary>
        /// Holds the instance telemetryClient.
        /// </summary>
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Holds the instance of project controller.
        /// </summary>
        private ProjectController projectController;

        /// <summary>
        /// Holds the instance of project helper.
        /// </summary>
        private Mock<IProjectHelper> projectHelper;

        /// <summary>
        /// Holds the instance of project helper.
        /// </summary>
        private Mock<IUserHelper> userHelper;

        /// <summary>
        /// The instance of task helper.
        /// </summary>
        private Mock<ITaskHelper> taskHelper;

        /// <summary>
        /// Holds the instance of manager dashboard helper.
        /// </summary>
        private Mock<IManagerDashboardHelper> managerDashboardHelper;

        /// <summary>
        /// Mocked instance of logger.
        /// </summary>
        private Mock<ILogger<ProjectController>> logger;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// The mocked instance of project repository.
        /// </summary>
        private Mock<IProjectRepository> projectRepository;

        /// <summary>
        /// The mocked instance of member repository.
        /// </summary>
        private Mock<IMemberRepository> memberRepository;

        /// <summary>
        /// The mocked instance of task repository.
        /// </summary>
        private Mock<ITaskRepository> taskRepository;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.logger = new Mock<ILogger<ProjectController>>();
            this.telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            this.projectHelper = new Mock<IProjectHelper>();
            this.userHelper = new Mock<IUserHelper>();
            this.managerDashboardHelper = new Mock<IManagerDashboardHelper>();
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.projectRepository = new Mock<IProjectRepository>();
            this.memberRepository = new Mock<IMemberRepository>();
            this.taskRepository = new Mock<ITaskRepository>();
            this.taskHelper = new Mock<ITaskHelper>();
            this.projectController = new ProjectController(this.logger.Object, this.projectHelper.Object, this.userHelper.Object, this.managerDashboardHelper.Object, this.telemetryClient, new TaskMapper(), this.taskHelper.Object, this.repositoryAccessors.Object);
            var httpContext = FakeHttpContext.MakeFakeContext();
            this.projectController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
        }

        /// <summary>
        /// Tests whether bad request status is return when end date is less than start date while fetching project utilization data.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectUtilization_WhenEndDateIsLessThanStartDate_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day - 1);

            // ACT
            var result = (ObjectResult)await this.projectController.GetProjectUtilizationAsync(projectId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Tests whether it returns HTTP status code BadRequest if project is not valid.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddCustomTaskForMemberAsync_WithInvalidProject_ReturnsStatusBadRequest()
        {
            var projectId = Guid.NewGuid();
            this.repositoryAccessors.Setup(ra => ra.ProjectRepository).Returns(() => this.projectRepository.Object);

            this.projectRepository.
                Setup(projectRepository => projectRepository.GetAsync(It.IsAny<Guid>())).
                Returns(Task.FromResult(TestData.Projects.Where(project => project.Id == projectId).FirstOrDefault()));

            this.memberRepository.
                Setup(memberRepository => memberRepository.GetAllActiveMembersAsync(It.IsAny<Guid>())).
                Returns(Task.FromResult(TestData.InvalidMembers));

            var timesheetDetails = new TimesheetDetails
            {
                EndDate = new DateTime(DateTime.UtcNow.Year, 1, 2),
                Hours = 3,
                IsAddedByMember = true,
                StartDate = new DateTime(DateTime.UtcNow.Year, 1, 2),
                TaskTitle = "New task",
            };

            var taskHelper = new TaskHelper(this.repositoryAccessors.Object, new Mock<ILogger<TaskHelper>>().Object);

            var addResult = (ObjectResult)await this.projectController.AddCustomTaskForMemberAsync(projectId, timesheetDetails);

            var error = (ErrorResponse)addResult.Value;
            Assert.AreEqual(StatusCodes.Status400BadRequest, addResult.StatusCode);
            Assert.AreEqual("Invalid project", error.Message);
        }

        /// <summary>
        /// Tests whether it returns HTTP status code NotFound if task is not found for deletion.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMemberTaskAsync_WithInvalidModel_ReturnsStatusNotFound()
        {
            this.repositoryAccessors.Setup(ra => ra.TaskRepository).Returns(() => this.taskRepository.Object);

            TaskEntity task = null;

            this.taskRepository.
                Setup(taskRepository => taskRepository.GetTask(It.IsAny<Guid>())).
                Returns(task);

            var deleteResult = (ObjectResult)await this.projectController.DeleteMemberTaskAsync(Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"), Guid.NewGuid());
            Assert.AreEqual(StatusCodes.Status404NotFound, deleteResult.StatusCode);
            var error = (ErrorResponse)deleteResult.Value;
            Assert.AreEqual("Task not found", error.Message);
        }

        /// <summary>
        /// Tests  whether not found status is return when project is not created by logged-in user while fetching project utilization data.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectUtilization_WhenProjectNotFound_ShouldReturnNotFoundStatus()
        {
            // ARRANGE
            ProjectUtilizationDTO project = null;

            this.projectHelper
                .Setup(helper => helper.GetProjectUtilizationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(project));

            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var result = (ObjectResult)await this.projectController.GetProjectUtilizationAsync(projectId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        /// <summary>
        /// Tests whether bad request status is return when null model is given while creating tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateTasks_WhenGivenNullModel_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            // ACT
            var result = (ObjectResult)await this.projectController.CreateTasksAsync(projectId, null);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Tests whether internal server error status is return when failure at database while creating tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateTasks_WhenFailureAtDatabase_ShouldReturnInternalServerStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();
            this.projectHelper
                .Setup(helper => helper.AddProjectTasksAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<TaskDTO>>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = (ObjectResult)await this.projectController.CreateTasksAsync(projectId, TestData.TaskDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        /// <summary>
        /// Tests whether created status is return with valid model while creating tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateTasks_WithValidModel_ShouldReturnCreatedStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();
            this.projectHelper
                .Setup(helper => helper.AddProjectTasksAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<TaskDTO>>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = (StatusCodeResult)await this.projectController.CreateTasksAsync(projectId, TestData.TaskDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status201Created, result.StatusCode);
        }

        /// <summary>
        /// Tests whether bad request status is return when null model is given while deleting tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTasks_WhenGivenNullModel_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            // ACT
            var result = (ObjectResult)await this.projectController.DeleteTasksFromProjectAsync(projectId, null);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Tests whether not found status is return when tasks not belongs to project while deleting tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTasks_WhenTasksNotBelongsToProject_ShouldReturnNotFoundStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();
            var taskIds = new List<Guid>() { Guid.NewGuid() };

            IEnumerable<TaskEntity> tasks = null;
            this.projectHelper
                .Setup(helper => helper.GetProjectTasksAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(tasks));

            // ACT
            var result = (StatusCodeResult)await this.projectController.DeleteTasksFromProjectAsync(projectId, taskIds);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        /// <summary>
        /// Tests whether no content status is return with valid model while deleting tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTasks_WithValidModel_ShouldReturnNoContentStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();
            var taskIds = new List<Guid>() { Guid.NewGuid() };

            this.projectHelper
                .Setup(helper => helper.GetProjectTasksAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(TestData.Tasks.AsEnumerable()));
            this.projectHelper
                .Setup(helper => helper.DeleteProjectTasksAsync(It.IsAny<List<TaskEntity>>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = (StatusCodeResult)await this.projectController.DeleteTasksFromProjectAsync(projectId, taskIds);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);
        }

        /// <summary>
        /// Tests whether internal server status is return when failure at database while deleting tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTasks_WhenFailureAtDatabase_ShouldReturnInternalServerErrorStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();
            var taskIds = new List<Guid>() { Guid.NewGuid() };

            this.projectHelper
                .Setup(helper => helper.GetProjectTasksAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(TestData.Tasks.AsEnumerable()));
            this.projectHelper
                .Setup(helper => helper.DeleteProjectTasksAsync(It.IsAny<List<TaskEntity>>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = (ObjectResult)await this.projectController.DeleteTasksFromProjectAsync(projectId, taskIds);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        /// <summary>
        /// Tests whether bad request is return status when end date is less than start date while fetching project task overview.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task GetProjectTasksOverview_WhenEndDateIsLessThanStartDate_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day - 1);

            // ACT
            var result = (ObjectResult)await this.projectController.GetProjectTasksOverviewAsync(projectId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Tests whether bad request status is return when null model is given while adding project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddProjectMembers_WhenGivenNullModel_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            // ACT
            var result = (ObjectResult)await this.projectController.AddProjectMembersAsync(projectId, null);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Tests whether unauthorized status is return when members aren't direct reportee of logged-in manager while adding project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddProjectMembers_WhenMembersAreNotDirectReportee_ShouldReturnUnauthorizedStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            this.userHelper
                .Setup(helper => helper.AreProjectMembersDirectReporteeAsync(It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = (StatusCodeResult)await this.projectController.AddProjectMembersAsync(projectId, TestData.MembersDTO);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status401Unauthorized, result.StatusCode);
        }

        /// <summary>
        /// Tests whether internal server status is return when failure at database while adding project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddProjectMembers_WhenFailureAtDatabase_ShouldReturnInternalServerErrorStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            this.userHelper
                .Setup(helper => helper.AreProjectMembersDirectReporteeAsync(It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(true));
            this.projectHelper
                .Setup(helper => helper.AddProjectMembersAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<MemberDTO>>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = (ObjectResult)await this.projectController.AddProjectMembersAsync(projectId, TestData.MembersDTO);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        /// <summary>
        /// Tests whether OK status is return with valid data while adding project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddProjectMembers_WithValidData_ShouldReturnOKStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            this.userHelper
                .Setup(helper => helper.AreProjectMembersDirectReporteeAsync(It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(true));
            this.projectHelper
                .Setup(helper => helper.AddProjectMembersAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<MemberDTO>>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = (StatusCodeResult)await this.projectController.AddProjectMembersAsync(projectId, TestData.MembersDTO);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }

        /// <summary>
        /// Tests whether bad request status is return when null model is passed while deleting members from project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMembersFromProject_WhenGivenNullModel_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            // ACT
            var result = (ObjectResult)await this.projectController.DeleteMembersFromProjectAsync(projectId, null);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Tests whether unauthorized status is return when members aren't direct reportee of logged-in manager while deleting members from project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMembersFromProject_WhenMembersAreNotDirectReportee_ShouldReturnUnauthorizedStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            this.userHelper
                .Setup(helper => helper.AreProjectMembersDirectReporteeAsync(It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = (StatusCodeResult)await this.projectController.DeleteMembersFromProjectAsync(projectId, TestData.ProjectMemberOverviewDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status401Unauthorized, result.StatusCode);
        }

        /// <summary>
        /// Tests whether unauthorized status is return when members aren't direct reportee of logged-in manager while deleting members from project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMembersFromProject_WhenMembersNotBelongsToProject_ShouldReturnNotFoundStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();
            IEnumerable<Member> members = null;

            this.userHelper
                .Setup(helper => helper.AreProjectMembersDirectReporteeAsync(It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(true));
            this.projectHelper
                .Setup(helper => helper.GetProjectMembersAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(members));

            // ACT
            var result = (StatusCodeResult)await this.projectController.DeleteMembersFromProjectAsync(projectId, TestData.ProjectMemberOverviewDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        /// <summary>
        /// Tests whether internal server error status is return when failure at database while deleting members from project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMembersFromProject_WhenFailureAtDatabase_ShouldReturnInternalServerErrorStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            this.userHelper
                .Setup(helper => helper.AreProjectMembersDirectReporteeAsync(It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(true));
            this.projectHelper
                .Setup(helper => helper.GetProjectMembersAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(TestData.Members.AsEnumerable()));
            this.projectHelper
                .Setup(helper => helper.DeleteProjectTasksAsync(It.IsAny<List<TaskEntity>>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = (ObjectResult)await this.projectController.DeleteMembersFromProjectAsync(projectId, TestData.ProjectMemberOverviewDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        /// <summary>
        /// Tests whether no content status is return with valid data while deleting members from project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMembersFromProject_WithValidData_ShouldReturnNoContentStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();

            this.userHelper
                .Setup(helper => helper.AreProjectMembersDirectReporteeAsync(It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(true));
            this.projectHelper
                .Setup(helper => helper.GetProjectMembersAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(Task.FromResult(TestData.Members.AsEnumerable()));
            this.projectHelper
                .Setup(helper => helper.DeleteProjectMembersAsync(It.IsAny<List<Member>>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = (StatusCodeResult)await this.projectController.DeleteMembersFromProjectAsync(projectId, TestData.ProjectMemberOverviewDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);
        }

        /// <summary>
        /// Tests whether bad request status is return when end date is less than start date while fetching project members overview.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectMembersOverview_WhenEndDateIsLessThanStartDate_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day - 1);

            // ACT
            var result = (ObjectResult)await this.projectController.GetProjectMembersOverviewAsync(projectId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Tests whether bad request status is return when end date is less than start date while fetching dashboard projects.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardProjects_WhenEndDateIsLessThanStartDate_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day - 1);

            // ACT
            var result = (ObjectResult)await this.projectController.GetDashboardProjectsAsync(startDate, endDate);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }
    }
}