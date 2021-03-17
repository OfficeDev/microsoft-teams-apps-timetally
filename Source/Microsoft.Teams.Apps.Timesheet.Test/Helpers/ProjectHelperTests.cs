// <copyright file="ProjectHelperTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Project helper tests contains all the test cases for methods managing projects.
    /// </summary>
    [TestClass]
    public class ProjectHelperTests
    {
        /// <summary>
        /// Project test model.
        /// </summary>
        private readonly Project project = new Project
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

        /// <summary>
        /// Instance of project helper.
        /// </summary>
        private ProjectHelper projectHelper;

        /// <summary>
        /// The mocked instance of timesheet repository.
        /// </summary>
        private Mock<ITimesheetRepository> timesheetRepository;

        /// <summary>
        /// The mocked instance of project repository.
        /// </summary>
        private Mock<IProjectRepository> projectRepository;

        /// <summary>
        /// The mocked instance of task repository.
        /// </summary>
        private Mock<ITaskRepository> taskRepository;

        /// <summary>
        /// The mocked instance of member repository.
        /// </summary>
        private Mock<IMemberRepository> memberRepository;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// The mocked instance of timesheet database context.
        /// </summary>
        private Mock<TimesheetContext> timesheetContext;

        /// <summary>
        /// The mocked instance of user service.
        /// </summary>
        private Mock<IUsersService> userService;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.timesheetContext = new Mock<TimesheetContext>();
            this.timesheetRepository = new Mock<ITimesheetRepository>();
            this.projectRepository = new Mock<IProjectRepository>();
            this.taskRepository = new Mock<ITaskRepository>();
            this.memberRepository = new Mock<IMemberRepository>();
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.userService = new Mock<IUsersService>();
            this.projectHelper = new ProjectHelper(this.timesheetContext.Object, this.repositoryAccessors.Object, new ProjectMapper(new Mock<ILogger<ProjectMapper>>().Object), new MemberMapper(), new TaskMapper());
        }

        /// <summary>
        /// Tests whether we can get project utilization data with valid parameter.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectById_WithValidParams_ShouldReturnValidData()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(this.project));

            var managerId = Guid.NewGuid();

            // ACT
            var project = await this.projectHelper.GetProjectByIdAsync(this.project.Id, managerId);

            // ASSERT
            Assert.AreEqual(TestData.ExpectedProjectDTO.Id, project.Id);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether null is return when project not found while fetching project details.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectById_WhenProjectNotFound_ShouldReturnNull()
        {
            // ARRANGE
            Project nullProject = null;
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(nullProject));

            var managerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            // ACT
            var project = await this.projectHelper.GetProjectByIdAsync(projectId, managerId);

            // ASSERT
            Assert.AreEqual(null, project);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can create project with valid model and valid project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateProject_WithValidModel_ShouldReturnValidProject()
        {
            // ARRANGE
            var projectDTO = new ProjectDTO
            {
                BillableHours = 50,
                NonBillableHours = 10,
                ClientName = "Samuel,",
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month, 28),
                Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                Title = "Project 1",
                Members = new List<MemberDTO>
                {
                    new MemberDTO
                    {
                        Id = Guid.NewGuid(),
                        IsBillable = true,
                        ProjectId = Guid.NewGuid(),
                        UserId = Guid.Parse("1ce072c1-1b87-4912-bb60-307698e6874e"),
                    },
                },
                Tasks = new List<TaskDTO>
                {
                    new TaskDTO
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = Guid.NewGuid(),
                        Title = "TaskEntity",
                    },
                },
            };
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());

            this.projectRepository
                .Setup(projectRepo => projectRepo.CreateProject(It.IsAny<Project>()))
                .Returns(this.project);
            this.userService
                .Setup(service => service.GetMyReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(TestData.Reportees.AsEnumerable()));
            this.timesheetContext
                .Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            var managerId = Guid.NewGuid();

            // ACT
            var result = await this.projectHelper.CreateProjectAsync(projectDTO, managerId);

            // ASSERT
            Assert.AreEqual(this.project.ClientName, result.ClientName);
            this.projectRepository.Verify(projectRepo => projectRepo.CreateProject(It.IsAny<Project>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether null exception is thrown when project details are null while creating project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateProject_WhenProjectDetailsAreNull_ShouldThrowNullException()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);

            ProjectDTO projectDetails = null;
            var managerId = Guid.NewGuid();

            try
            {
                // ACT
                var project = await this.projectHelper.CreateProjectAsync(projectDetails, managerId);
            }
            catch (ArgumentNullException exception)
            {
                // ASSERT
                Assert.AreEqual(nameof(projectDetails), exception.ParamName);
                this.projectRepository.Verify(projectRepo => projectRepo.CreateProject(It.IsAny<Project>()), Times.Never());
            }
        }

        /// <summary>
        /// Tests whether true is return when project is updated.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task UpdateProject_WithValidModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(this.project));
            this.projectRepository
                .Setup(projectRepo => projectRepo.Update(It.IsAny<Project>()))
                .Returns(this.project);
            this.timesheetContext
                .Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            var managerId = this.project.CreatedBy;

            // ACT
            var result = await this.projectHelper.UpdateProjectAsync(TestData.ProjectUpdateDTO, managerId);

            // ASSERT
            Assert.IsTrue(result);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
            this.projectRepository.Verify(projectRepo => projectRepo.Update(It.IsAny<Project>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether argument exception is thrown when project details are null while updating project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task UpdateProject_WhenProjectDetailsAreNull_ShouldThrowArgumentException()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);

            ProjectUpdateDTO nullProjectUpdateDTO = null;
            var managerId = Guid.NewGuid();

            try
            {
                // ACT
                var isUpdated = await this.projectHelper.UpdateProjectAsync(nullProjectUpdateDTO, managerId);
            }
            catch (ArgumentException exception)
            {
                // ASSERT
                Assert.AreEqual("The project details must be provided.", exception.Message);
                this.projectRepository.Verify(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
                this.projectRepository.Verify(projectRepo => projectRepo.Update(It.IsAny<Project>()), Times.Never());
            }
        }

        /// <summary>
        /// Tests whether false is return when failure at database while updating project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task UpdateProject_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(this.project));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            var managerId = Guid.NewGuid();

            // ACT
            var result = await this.projectHelper.UpdateProjectAsync(TestData.ProjectUpdateDTO, managerId);

            // ASSERT
            Assert.IsFalse(result);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
            this.projectRepository.Verify(projectRepo => projectRepo.Update(It.IsAny<Project>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can get project utilization data with valid parameter.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectUtilization_WithValidParams_ShouldReturnValidData()
        {
            // ARRANGE
            var members = new List<Member>
            {
                new Member
                {
                    ProjectId = Guid.NewGuid(),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    IsBillable = true,
                    Id = Guid.Parse("54ab7412-f6c1-491d-be16-f797e6903667"),
                },
            };
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(this.project));
            this.memberRepository
                .Setup(projectRepo => projectRepo.GetAllMembers(It.IsAny<Guid>()))
                .Returns(members);
            var managerId = Guid.NewGuid().ToString();
            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectUtilization = await this.projectHelper.GetProjectUtilizationAsync(projectId, managerId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(TestData.ExpectedProjectUtilization.Id, projectUtilization.Id);
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests  whether null is return when project is not created by logged-in user while fetching project utilization data.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectUtilization_WhenProjectNotFound_ShouldReturnNull()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);

            Project project = null;
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(project));

            var managerId = Guid.NewGuid().ToString();
            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectUtilization = await this.projectHelper.GetProjectUtilizationAsync(projectId, managerId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(null, projectUtilization);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never());
        }

        /// <summary>
        /// Tests whether true is return on successful creation of tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateTasks_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.taskRepository.Setup(taskRepo => taskRepo.CreateTasksAsync(It.IsAny<IEnumerable<TaskEntity>>())).Returns(Task.FromResult(true));
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(TestData.TaskDTOs.Count));

            var projectId = Guid.NewGuid();

            // ACT
            var isAdded = await this.projectHelper.AddProjectTasksAsync(projectId, TestData.TaskDTOs);

            // ASSERT
            Assert.IsTrue(isAdded);
            this.taskRepository.Verify(taskRepo => taskRepo.CreateTasksAsync(It.IsAny<IEnumerable<TaskEntity>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether argument exception is thrown when null model is given while creating tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateTasks_WhenGivenNullModel_ShouldThrowArgumentException()
        {
            try
            {
                // ARRANGE
                var projectId = Guid.NewGuid();

                // ACT
                var isAdded = await this.projectHelper.AddProjectTasksAsync(projectId, null);
            }
            catch (ArgumentException exception)
            {
                // ASSERT
                Assert.AreEqual("Task list is either null or empty.", exception.Message);
                this.taskRepository.Verify(taskRepo => taskRepo.CreateTasksAsync(It.IsAny<IEnumerable<TaskEntity>>()), Times.Never());
            }
        }

        /// <summary>
        /// Tests whether false is return when failure at database while creating tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateTasks_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.taskRepository.Setup(taskRepo => taskRepo.CreateTasksAsync(It.IsAny<IEnumerable<TaskEntity>>())).Returns(Task.FromResult(true));
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            // ACT
            var isAdded = await this.projectHelper.AddProjectTasksAsync(Guid.NewGuid(), TestData.TaskDTOs);

            // ASSERT
            Assert.IsFalse(isAdded);
            this.taskRepository.Verify(taskRepo => taskRepo.CreateTasksAsync(It.IsAny<IEnumerable<TaskEntity>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether true is return on successful deletion of tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTasks_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.taskRepository
                .Setup(taskRepo => taskRepo.FindAsync(It.IsAny<Expression<Func<TaskEntity, bool>>>()))
                .Returns(Task.FromResult(TestData.Tasks as IEnumerable<TaskEntity>));
            this.taskRepository
                .Setup(taskRepo => taskRepo.UpdateTasks(It.IsAny<List<TaskEntity>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            var projectId = Guid.NewGuid();

            // ACT
            var operationResult = await this.projectHelper.DeleteProjectTasksAsync(TestData.Tasks);

            // ASSERT
            Assert.IsTrue(operationResult);
            this.taskRepository.Verify(taskRepo => taskRepo.UpdateTasks(It.IsAny<List<TaskEntity>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether false is return when failure at database while deleting tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTasks_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.taskRepository
                .Setup(taskRepo => taskRepo.FindAsync(It.IsAny<Expression<Func<TaskEntity, bool>>>()))
                .Returns(Task.FromResult(TestData.Tasks as IEnumerable<TaskEntity>));
            this.taskRepository
                .Setup(taskRepo => taskRepo.UpdateTasks(It.IsAny<List<TaskEntity>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            var projectId = Guid.NewGuid();

            // ACT
            var operationResult = await this.projectHelper.DeleteProjectTasksAsync(TestData.Tasks);

            // ASSERT
            Assert.IsFalse(operationResult);
            this.taskRepository.Verify(taskRepo => taskRepo.UpdateTasks(It.IsAny<List<TaskEntity>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can get project tasks overview data with valid parameter.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectTasksOverview_WithValidParams_ShouldReturnValidData()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.taskRepository
                .Setup(taskRepo => taskRepo.GetTasksByProjectIdAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Tasks));

            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectTasksOverview = await this.projectHelper.GetProjectTasksOverviewAsync(projectId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(TestData.ExpectedProjectTasksOverview.Count(), projectTasksOverview.Count());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
            this.taskRepository.Verify(taskRepo => taskRepo.GetTasksByProjectIdAsync(It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether empty list is return when tasks is not found while fetching project task overview.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectTasksOverview_WhenTasksNotFound_ShouldReturnEmptyList()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.taskRepository
                .Setup(taskRepo => taskRepo.GetTasksByProjectIdAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new List<TaskEntity>().AsEnumerable()));

            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectTasksOverview = await this.projectHelper.GetProjectTasksOverviewAsync(projectId, startDate, endDate);

            // ASSERT
            Assert.IsTrue(projectTasksOverview.IsNullOrEmpty());
            this.taskRepository.Verify(taskRepo => taskRepo.GetTasksByProjectIdAsync(It.IsAny<Guid>()), Times.AtLeastOnce());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never());
        }

        /// <summary>
        /// Tests whether true is return on successfully adding project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddProjectMembers_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetAllMembers(It.IsAny<Guid>()))
                .Returns(TestData.Members.ToList());
            this.memberRepository
                .Setup(memberRepo => memberRepo.AddUsersAsync(It.IsAny<List<Member>>()))
                .Returns(Task.FromResult(true));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // ACT
            var resultResponse = await this.projectHelper.AddProjectMembersAsync(Guid.NewGuid(), TestData.MembersDTO);

            // ASSERT
            Assert.IsTrue(resultResponse);
            this.memberRepository.Verify(memberRepo => memberRepo.AddUsersAsync(It.IsAny<IEnumerable<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether false is return when failure at database while adding project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddProjectMembers_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetAllMembers(It.IsAny<Guid>()))
                .Returns(TestData.Members.ToList());
            this.memberRepository
                .Setup(memberRepo => memberRepo.AddUsersAsync(It.IsAny<List<Member>>()))
                .Returns(Task.FromResult(true));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            // ACT
            var resultResponse = await this.projectHelper.AddProjectMembersAsync(Guid.NewGuid(), TestData.MembersDTO);

            // ASSERT
            Assert.IsFalse(resultResponse);
            this.memberRepository.Verify(memberRepo => memberRepo.AddUsersAsync(It.IsAny<IEnumerable<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can add project members with correct model who were removed earlier and get result true.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddExistingUsers_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetAllMembers(It.IsAny<Guid>()))
                .Returns(TestData.Members.ToList());
            this.memberRepository
                .Setup(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(TestData.Members.Count()));

            // ACT
            var resultResponse = await this.projectHelper.AddProjectMembersAsync(Guid.NewGuid(), TestData.ExistingMembers);

            // ASSERT
            Assert.IsTrue(resultResponse);
            this.memberRepository.Verify(memberRepo => memberRepo.UpdateMembers(It.IsAny<IEnumerable<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether on successfully deleting members return true.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMembersFromProject_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());

            this.memberRepository
                .Setup(memberRepo => memberRepo.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
                .Returns(Task.FromResult(TestData.Members.AsEnumerable()));
            this.memberRepository
                .Setup(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(TestData.Members.Count()));

            var projectId = Guid.NewGuid();

            // ACT
            var operationResult = await this.projectHelper.DeleteProjectMembersAsync(TestData.Members);

            // ASSERT
            Assert.IsTrue(operationResult);
            this.memberRepository.Verify(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether false is return if there is failure at database while deleting project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMembersFromProject_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());

            this.memberRepository
                .Setup(memberRepo => memberRepo.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
                .Returns(Task.FromResult(TestData.Members.AsEnumerable()));
            this.memberRepository
                .Setup(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            var projectId = Guid.NewGuid();

            // ACT
            var operationResult = await this.projectHelper.DeleteProjectMembersAsync(TestData.Members);

            // ASSERT
            Assert.IsFalse(operationResult);
            this.memberRepository.Verify(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can get project members overview with valid parameters.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectMembersOverview_WithValidParams_ShouldReturnOKStatusWithValidData()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);

            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetAllActiveMembersAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Members));

            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectMembersOverview = await this.projectHelper.GetProjectMembersOverviewAsync(projectId, startDate, endDate);

            // ASSERT
            Assert.IsFalse(projectMembersOverview.IsNullOrEmpty());
            Assert.AreEqual(TestData.ExpectedProjectMembersOverview.Count(), projectMembersOverview.Count());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
            this.memberRepository.Verify(memberRepo => memberRepo.GetAllActiveMembersAsync(It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether empty list is return when members not found while fetching project members overview.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetProjectMembersOverview_WhenMembersNotFound_ShouldReturnEmptyList()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);

            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetAllActiveMembersAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Enumerable.Empty<Member>()));

            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectMembersOverview = await this.projectHelper.GetProjectMembersOverviewAsync(projectId, startDate, endDate);

            // ASSERT
            Assert.IsTrue(projectMembersOverview.IsNullOrEmpty());
            this.memberRepository.Verify(memberRepo => memberRepo.GetAllActiveMembersAsync(It.IsAny<Guid>()), Times.AtLeastOnce());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never());
        }
    }
}