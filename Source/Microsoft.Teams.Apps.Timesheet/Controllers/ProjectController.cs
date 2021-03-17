// <copyright file="ProjectController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Project controller is responsible to expose API endpoints for performing CRUD operation on project entity.
    /// </summary>
    [Route("api/projects")]
    [ApiController]
    [Authorize]
    public class ProjectController : BaseController
    {
        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The instance of project helper which helps in managing operations on project entity.
        /// </summary>
        private readonly IProjectHelper projectHelper;

        /// <summary>
        /// Instance of user helper.
        /// </summary>
        private readonly IUserHelper userHelper;

        /// <summary>
        /// Holds the instance of task mapper.
        /// </summary>
        private readonly ITaskMapper taskMapper;

        /// <summary>
        /// Holds the instance of task helper.
        /// </summary>
        private readonly ITaskHelper taskHelper;

        /// <summary>
        /// Holds the instance of manager dashboard helper which helps in managing operations on dashboard entity.
        /// </summary>
        private readonly IManagerDashboardHelper managerDashboardHelper;

        /// <summary>
        /// Holds the instance of respository accessors for database operations.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information.</param>
        /// <param name="projectHelper">The instance of project helper which helps in managing operations on project entity.</param>
        /// <param name="userHelper">Instance of user helper.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="taskMapper">The instance of task mapper.</param>
        /// <param name="taskHelper">The instance of task helper.</param>
        /// <param name="managerDashboardHelper">Holds the instance of manager dashboard helper.</param>
        /// <param name="repositoryAccessors">Holds the instance of respository accessors for database operations.</param>
        public ProjectController(
            ILogger<ProjectController> logger,
            IProjectHelper projectHelper,
            IUserHelper userHelper,
            IManagerDashboardHelper managerDashboardHelper,
            TelemetryClient telemetryClient,
            ITaskMapper taskMapper,
            ITaskHelper taskHelper,
            IRepositoryAccessors repositoryAccessors)
            : base(telemetryClient)
        {
            this.projectHelper = projectHelper;
            this.userHelper = userHelper;
            this.managerDashboardHelper = managerDashboardHelper;
            this.logger = logger;
            this.taskMapper = taskMapper;
            this.taskHelper = taskHelper;
            this.repositoryAccessors = repositoryAccessors;
        }

        /// <summary>
        /// Get a project by project Id.
        /// </summary>
        /// <param name="id">Unique project Id.</param>
        /// <returns>Returns list of projects.</returns>
        [HttpGet("{id}")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        [Authorize(PolicyNames.MustBeProjectCreatorPolicy)]
        public async Task<IActionResult> GetProjectAsync(Guid id)
        {
            this.RecordEvent("Get project- The HTTP GET call to get project details has been initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "projectId", id.ToString() },
            });

            if (id == Guid.Empty)
            {
                this.RecordEvent("Get project- The HTTP GET call to get project details has failed.", RequestType.Failed);
                return this.BadRequest("Invalid project Id.");
            }

            try
            {
                var projectDetails = await this.projectHelper.GetProjectByIdAsync(id, Guid.Parse(this.UserAadId));

                if (projectDetails == null)
                {
                    this.RecordEvent("Get project- The HTTP GET call to get project details has failed.", RequestType.Failed);
                    return this.NotFound("Project not found.");
                }
                else
                {
                    this.RecordEvent("Get project- The HTTP GET call to get project details has succeeded.", RequestType.Succeeded);
                    return this.Ok(projectDetails);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get project- The HTTP GET call to get project details has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching project.");
                throw;
            }
        }

        /// <summary>
        /// Create a new project.
        /// </summary>
        /// <param name="projectDetails">The details of project to be created.</param>
        /// <returns>Returns project details that was created.</returns>
        [HttpPost]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        public async Task<IActionResult> CreateProjectAsync([FromBody] ProjectDTO projectDetails)
        {
            this.RecordEvent("Create project- The HTTP POST call has initiated.", RequestType.Initiated);
            if (projectDetails == null)
            {
                this.logger.LogError("Project detail is null.");
                this.RecordEvent("Create project- The HTTP POST call has failed.", RequestType.Failed);
                return this.BadRequest();
            }

            try
            {
                // Validate if project members are direct reportees to logged-in user.
                // If yes, save new project and return HTTP status code Created.
                var projectMemberIds = projectDetails.Members.Select(member => member.UserId);
                var areDirectReportee = await this.userHelper.AreProjectMembersDirectReporteeAsync(projectMemberIds);
                if (!areDirectReportee)
                {
                    this.logger.LogError("Project members are not direct reportee of manager.");
                    this.RecordEvent("Create project- The HTTP POST call has failed.", RequestType.Failed);
                    return this.Unauthorized();
                }

                var createResult = await this.projectHelper.CreateProjectAsync(projectDetails, Guid.Parse(this.UserAadId));

                if (createResult == null)
                {
                    this.RecordEvent("Create project- The HTTP POST call has failed.", RequestType.Failed);
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }

                this.RecordEvent("Create project- The HTTP POST call has succeeded.", RequestType.Succeeded);

                return this.StatusCode((int)HttpStatusCode.Created, createResult);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Create project- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while creating project.");
                throw;
            }
        }

        /// <summary>
        /// Update a project.
        /// </summary>
        /// <param name="projectId">Project Id.</param>
        /// <param name="projectDetails">The details of project to be updated.</param>
        /// <returns>Returns NoContent HTTP status on successful operation.</returns>
        [HttpPatch("{projectId}")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        [Authorize(PolicyNames.MustBeProjectCreatorPolicy)]
        public async Task<IActionResult> UpdateProjectAsync([FromQuery] Guid projectId, [FromBody] ProjectUpdateDTO projectDetails)
        {
            this.RecordEvent("Update project- The HTTP PATCH call has initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "projectId", Convert.ToString(projectId, CultureInfo.InvariantCulture) },
            });

#pragma warning disable CA1062 // Null check is handled by data annotations.
            projectDetails.Id = projectId;
#pragma warning restore CA1062 // Null check is handled by data annotations.

            try
            {
                var updateResult = await this.projectHelper.UpdateProjectAsync(projectDetails, Guid.Parse(this.UserAadId));

                if (!updateResult)
                {
                    this.RecordEvent("Update project- The HTTP PATCH call has failed.", RequestType.Failed);
                    return this.StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse { Message = "Error occurred while updating project." });
                }

                this.RecordEvent("Update project- The HTTP PATCH call has succeeded.", RequestType.Succeeded);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                this.RecordEvent("Update project- The HTTP PATCH call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while updating project.");
                throw;
            }
        }

        /// <summary>
        /// Handles request to add new task created by project member.
        /// </summary>
        /// <param name="projectId">The project Id.</param>
        /// <param name="timesheetDetails">The timesheet details.</param>
        /// <returns>Returns new task details if task created successfully. Else return null.</returns>
        [HttpPost("{projectId}/member/tasks")]
        [Authorize(Policy = PolicyNames.MustBeProjectMemberPolicy)]
        public async Task<IActionResult> AddCustomTaskForMemberAsync(Guid projectId, [FromBody] TimesheetDetails timesheetDetails)
        {
            this.RecordEvent("Add task- The HTTP POST call to add new task has been initiated.", RequestType.Initiated);

            if (projectId == Guid.Empty)
            {
                this.RecordEvent("Add task- The HTTP POST call to add new task has been failed.", RequestType.Failed);
                return this.BadRequest("The project Id must be provided in order to create a task.");
            }

            if (timesheetDetails == null)
            {
                this.RecordEvent("Add task- The HTTP POST call to add new task has been failed.", RequestType.Failed);
                return this.BadRequest("Task details were not provided.");
            }

            try
            {
                var projectDetails = await this.repositoryAccessors.ProjectRepository.GetAsync(projectId);

                if (projectDetails == null)
                {
                    this.logger.LogInformation("Project details not found");
                    return this.BadRequest(new ErrorResponse { Message = "Invalid project" });
                }

                if (timesheetDetails.StartDate < projectDetails.StartDate.Date || timesheetDetails.EndDate > projectDetails.EndDate.Date)
                {
                    this.logger.LogInformation("Task start and end date is not within project start and end date");
                    return this.BadRequest(new ErrorResponse { Message = "Invalid start and end date for task" });
                }

                var taskDetails = this.taskMapper.MapForCreateModel(timesheetDetails, projectId, Guid.Parse(this.UserAadId));
                var creationResult = await this.taskHelper.AddMemberTaskAsync(taskDetails, projectId, Guid.Parse(this.UserAadId));

                if (creationResult != null)
                {
                    this.RecordEvent("Add task- The HTTP POST call to add new task has been succeeded.", RequestType.Succeeded);

                    var taskViewModel = this.taskMapper.MapForViewModel(creationResult);
                    return this.Ok(taskViewModel);
                }

                this.RecordEvent("Add task- The HTTP POST call to add new task has been failed.", RequestType.Failed);
                return this.StatusCode((int)HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Add task- The HTTP POST call to add new task has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while creating a new task.");
                throw;
            }
        }

        /// <summary>
        /// Handles request to delete a task created by project member.
        /// </summary>
        /// <param name="projectId">Project for which task needs to be deleted.</param>
        /// <param name="taskId">The task Id to be deleted.</param>
        /// <returns>Returns true if task deleted successfully. Else returns false.</returns>
        [HttpDelete("{projectId}/tasks/{taskId}")]
        [Authorize(Policy = PolicyNames.MustBeProjectMemberPolicy)]
        public async Task<IActionResult> DeleteMemberTaskAsync(Guid projectId, Guid taskId)
        {
            this.RecordEvent("Delete member task- The HTTP DELETE call to delete member task has been initiated.", RequestType.Initiated);

            if (taskId == Guid.Empty)
            {
                this.RecordEvent("Delete member task- The HTTP DELETE call to delete member task has been failed.", RequestType.Failed);
                return this.BadRequest("The valid task Id must be provided in order to delete a task.");
            }

            if (projectId == Guid.Empty)
            {
                this.RecordEvent("Delete member task- The HTTP DELETE call to delete member task has been failed.", RequestType.Failed);
                return this.BadRequest("The valid project Id must be provided in order to delete a task.");
            }

            try
            {
                var taskDetails = this.repositoryAccessors.TaskRepository.GetTask(taskId);

                // Do not allow to delete task, if
                // 1. Task is not added by project member.
                // 2. Logged-in user is not the one who created a task.
                if (taskDetails == null || !taskDetails.IsAddedByMember || taskDetails.MemberMapping?.UserId != Guid.Parse(this.UserAadId) || taskDetails.ProjectId != projectId)
                {
                    this.logger.LogInformation("Task not found");
                    return this.NotFound(new ErrorResponse { Message = "Task not found" });
                }

                var deletionResult = await this.taskHelper.DeleteMemberTaskAsync(taskId);

                if (deletionResult)
                {
                    this.RecordEvent("Delete member task- The HTTP DELETE call to delete member task has been succeeded.", RequestType.Succeeded);
                    return this.Ok();
                }

                this.RecordEvent("Delete member task- The HTTP DELETE call to delete member task has been failed.", RequestType.Failed);
                return this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to delete task");
            }
            catch (Exception ex)
            {
                this.RecordEvent("Delete member task- The HTTP DELETE call to delete member task has been failed.", RequestType.Failed);

                this.logger.LogError(ex, "Error occurred while deleting member task.");
                throw;
            }
        }

        /// <summary>
        /// Get project utilization details between date range for a project.
        /// </summary>
        /// <param name="projectId">The project Id of which details to fetch.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns project utilization detail.</returns>
        [HttpGet("{projectId}/utilization")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        [Authorize(PolicyNames.MustBeProjectCreatorPolicy)]
        public async Task<IActionResult> GetProjectUtilizationAsync(Guid projectId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            this.RecordEvent("Get project utilization- The HTTP GET call has been initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "projectId", projectId.ToString() },
                { "startDate", startDate.ToString("O", CultureInfo.InvariantCulture) },
                { "endDate", endDate.ToString("O", CultureInfo.InvariantCulture) },
            });

            if (endDate < startDate)
            {
                this.logger.LogError("End date is less than start date.");
                this.RecordEvent("Get project utilization- The HTTP GET call has been failed.", RequestType.Failed);
                return this.BadRequest(new ErrorResponse { Message = "End date is less than start date." });
            }

            try
            {
                var projectUtilization = await this.projectHelper.GetProjectUtilizationAsync(projectId, this.UserAadId, startDate.Date, endDate.Date);

                if (projectUtilization == null)
                {
                    this.RecordEvent("Get project utilization- The HTTP GET call has been failed.", RequestType.Failed);
                    this.logger.LogInformation("Project not found for logged in manager.", projectId);
                    return this.NotFound(new ErrorResponse { Message = "Project not found." });
                }

                this.RecordEvent("Get project utilization- The HTTP GET call has been succeeded.", RequestType.Succeeded);

                return this.Ok(projectUtilization);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get project utilization- The HTTP GET call has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching projects utilization.");
                throw;
            }
        }

        /// <summary>
        /// Handles request for adding members to a project.
        /// </summary>
        /// <param name="projectId">The Id of the project in which members need to be added.</param>
        /// <param name="members">The details of users to be added.</param>
        /// <returns>Returns HTTP status code OK on successful operation.</returns>
        [HttpPost("{projectId}/members")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        [Authorize(PolicyNames.MustBeProjectCreatorPolicy)]
        public async Task<IActionResult> AddProjectMembersAsync(Guid projectId, [FromBody] IEnumerable<MemberDTO> members)
        {
            this.RecordEvent("Add project members- The HTTP POST call has initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "projectId", projectId.ToString() },
            });

            if (members.IsNullOrEmpty())
            {
                this.RecordEvent("Add project members- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError("Members are either null or empty.");
                return this.BadRequest(new ErrorResponse { Message = "Members are either null or empty." });
            }

            try
            {
                var projectMemberIds = members.Select(member => member.UserId);

                // Validate all members are direct reportee of logged-in manager.
                var areDirectReportee = await this.userHelper.AreProjectMembersDirectReporteeAsync(projectMemberIds);

                if (!areDirectReportee)
                {
                    this.logger.LogError("Project members are not direct reportee of manager.");
                    this.RecordEvent("Add project members- The HTTP POST call has failed.", RequestType.Failed);
                    return this.Unauthorized();
                }

                var operationResult = await this.projectHelper.AddProjectMembersAsync(projectId, members);

                if (operationResult)
                {
                    this.RecordEvent("Add project members- The HTTP POST call has succeeded.", RequestType.Succeeded);

                    return this.Ok();
                }
                else
                {
                    this.RecordEvent("Add project members- The HTTP POST call has failed.", RequestType.Failed);

                    return this.StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse { Message = "Unable to add project members." });
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("Add project members- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while adding members.");
                throw;
            }
        }

        /// <summary>
        /// Handles request for adding tasks to a project.
        /// </summary>
        /// <param name="projectId">The Id of the project in which tasks need to be created.</param>
        /// <param name="tasks">The details of tasks to be created.</param>
        /// <returns>Returns HTTP status code created on successful operation.</returns>
        [HttpPost("{projectId}/tasks")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        [Authorize(PolicyNames.MustBeProjectCreatorPolicy)]
        public async Task<IActionResult> CreateTasksAsync(Guid projectId, [FromBody] IEnumerable<TaskDTO> tasks)
        {
            this.RecordEvent("Create tasks- The HTTP POST call has initiated", RequestType.Initiated, new Dictionary<string, string>
            {
                { "projectId", projectId.ToString() },
            });

            if (tasks.IsNullOrEmpty())
            {
                this.RecordEvent("Create tasks- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError("Tasks are either null or empty.");
                return this.BadRequest(new ErrorResponse { Message = "Tasks are either null or empty." });
            }

            try
            {
                var isTaskCreated = await this.projectHelper.AddProjectTasksAsync(projectId, tasks);
                if (isTaskCreated)
                {
                    this.RecordEvent("Create tasks- The HTTP POST call has succeeded.", RequestType.Succeeded);
                    return this.StatusCode((int)HttpStatusCode.Created);
                }
                else
                {
                    this.RecordEvent("Create tasks- The HTTP POST call has failed.", RequestType.Failed);
                    return this.StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse { Message = "Failed to create task in project." });
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("Create tasks- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while creating tasks.");
                throw;
            }
        }

        /// <summary>
        /// Handles request to remove members from a project.
        /// </summary>
        /// <param name="projectId">The Id of the project in which members need to be updated.</param>
        /// <param name="members">The details of users to be updated.</param>
        /// <returns>Returns HTTP status code NoContent on successful operation.</returns>
        [HttpPost("{projectId}/deletemembers")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        [Authorize(PolicyNames.MustBeProjectCreatorPolicy)]
        public async Task<IActionResult> DeleteMembersFromProjectAsync(Guid projectId, [FromBody] IEnumerable<ProjectMemberOverviewDTO> members)
        {
            this.RecordEvent("Delete members- The HTTP POST call has initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "projectId", projectId.ToString() },
            });

            if (members.IsNullOrEmpty())
            {
                this.RecordEvent("Delete members- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError("Members are either null or empty.");
                return this.BadRequest(new ErrorResponse { Message = "Members are either null or empty." });
            }

            try
            {
                var projectGraphIds = members.Select(member => member.UserId);

                // Validate all members are direct reportee of logged-in manager.
                var areDirectReportee = await this.userHelper.AreProjectMembersDirectReporteeAsync(projectGraphIds);

                if (!areDirectReportee)
                {
                    this.logger.LogError("Project members are not direct reportee of manager.");
                    this.RecordEvent("Delete members- The HTTP POST call has failed.", RequestType.Failed);
                    return this.Unauthorized();
                }

                var projectMemberIds = members.Select(member => member.Id);

                // Validate all members are part of project, else return null.
                var projectMembers = await this.projectHelper.GetProjectMembersAsync(projectId, projectMemberIds);

                if (projectMembers == null)
                {
                    this.logger.LogError("One or more members not found in project.");
                    this.RecordEvent("Delete members- The HTTP POST call has failed.", RequestType.Failed);
                    return this.NotFound();
                }

                var operationResult = await this.projectHelper.DeleteProjectMembersAsync(projectMembers);

                if (operationResult)
                {
                    this.RecordEvent("Delete members- The HTTP POST call has succeeded.", RequestType.Succeeded);
                    return this.NoContent();
                }
                else
                {
                    this.RecordEvent("Delete members- The HTTP POST call has failed.", RequestType.Failed);
                    return this.StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse { Message = "Unable to delete members." });
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("Delete members- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while deleting members.");
                throw;
            }
        }

        /// <summary>
        /// Handles request to delete tasks from a project.
        /// </summary>
        /// <param name="projectId">The Id of the project in which members need to be updated.</param>
        /// <param name="taskIds">The details of tasks to be deleted.</param>
        /// <returns>Returns HTTP status code NoContent on successful operation.</returns>
        [HttpPost("{projectId}/deletetasks")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        [Authorize(PolicyNames.MustBeProjectCreatorPolicy)]
        public async Task<IActionResult> DeleteTasksFromProjectAsync(Guid projectId, [FromBody] IEnumerable<Guid> taskIds)
        {
            this.RecordEvent("Delete tasks- The HTTP POST call has initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "projectId", projectId.ToString() },
            });

            if (taskIds.IsNullOrEmpty())
            {
                this.RecordEvent("Delete tasks- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError("Tasks are either null or empty.");
                return this.BadRequest(new ErrorResponse { Message = "Tasks are either null or empty." });
            }

            try
            {
                // Validate all tasks are part of project, else return null.
                var projectTasks = await this.projectHelper.GetProjectTasksAsync(projectId, taskIds);

                if (projectTasks == null)
                {
                    this.logger.LogError("One or more tasks not found in project.");
                    this.RecordEvent("Delete tasks- The HTTP POST call has failed.", RequestType.Failed);
                    return this.NotFound();
                }

                var operationResult = await this.projectHelper.DeleteProjectTasksAsync(projectTasks);

                if (operationResult)
                {
                    this.RecordEvent("Delete tasks- The HTTP POST call has succeeded.", RequestType.Succeeded);
                    return this.NoContent();
                }
                else
                {
                    this.RecordEvent("Delete tasks- The HTTP POST call has failed.", RequestType.Failed);
                    return this.StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse { Message = "Unable to delete tasks." });
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("Delete tasks- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while deleting tasks.");
                throw;
            }
        }

        /// <summary>
        /// Get members overview for a project.
        /// Overview contains member information along with burned efforts.
        /// </summary>
        /// <param name="projectId">The project Id of which details to fetch.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of project members overview.</returns>
        [HttpGet("{projectId}/membersoverview")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        [Authorize(PolicyNames.MustBeProjectCreatorPolicy)]
        public async Task<IActionResult> GetProjectMembersOverviewAsync(Guid projectId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            this.RecordEvent("Get project members overview- The HTTP GET call has been initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "projectId", projectId.ToString() },
                { "startDate", startDate.ToString("O", CultureInfo.InvariantCulture) },
                { "endDate", endDate.ToString("O", CultureInfo.InvariantCulture) },
            });

            if (endDate < startDate)
            {
                this.logger.LogError("End date is less than start date.");
                this.RecordEvent("Get project members overview- The HTTP GET call has been failed.", RequestType.Failed);
                return this.BadRequest(new ErrorResponse { Message = "End date is less than start date." });
            }

            try
            {
                var membersOverview = await this.projectHelper.GetProjectMembersOverviewAsync(projectId, startDate.Date, endDate.Date);

                this.RecordEvent("Get project members overview- The HTTP GET call has been succeeded.", RequestType.Succeeded);

                return this.Ok(membersOverview);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get project members overview- The HTTP GET call has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching projects members overview.");
                throw;
            }
        }

        /// <summary>
        /// Get tasks overview for a project.
        /// Overview contains task information along with burned efforts.
        /// </summary>
        /// <param name="projectId">The project Id of which details to fetch.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of project tasks overview.</returns>
        [HttpGet("{projectId}/tasksoverview")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        [Authorize(PolicyNames.MustBeProjectCreatorPolicy)]
        public async Task<IActionResult> GetProjectTasksOverviewAsync(Guid projectId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            this.RecordEvent("Get project tasks overview- The HTTP GET call has been initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "projectId", projectId.ToString() },
                { "startDate", startDate.ToString("O", CultureInfo.InvariantCulture) },
                { "endDate", endDate.ToString("O", CultureInfo.InvariantCulture) },
            });

            if (endDate < startDate)
            {
                this.logger.LogError("End date is less than start date.");
                this.RecordEvent("Get project tasks overview - The HTTP GET call has been failed.", RequestType.Failed);
                return this.BadRequest(new ErrorResponse { Message = "End date is less than start date." });
            }

            try
            {
                var taskOverview = await this.projectHelper.GetProjectTasksOverviewAsync(projectId, startDate.Date, endDate.Date);

                this.RecordEvent("Get project tasks overview- The HTTP GET call has been succeeded.", RequestType.Succeeded);

                return this.Ok(taskOverview);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get project tasks overview- The HTTP GET call has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occured while fetching projects tasks overview.");
                throw;
            }
        }

        /// <summary>
        /// Get approved and active project details for dashboard between date range.
        /// </summary>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of dashboard projects.</returns>
        [HttpGet("dashboard")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        public async Task<IActionResult> GetDashboardProjectsAsync([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            this.RecordEvent("Get dashboard projects- The HTTP call to GET dashboard projects has been initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "startDate", startDate.ToString("O", CultureInfo.InvariantCulture) },
                { "endDate", endDate.ToString("O", CultureInfo.InvariantCulture) },
            });

            try
            {
                if (endDate < startDate)
                {
                    this.logger.LogError("End date is less than start date.");
                    this.RecordEvent("Get dashboard projects- The HTTP call to GET dashboard projects has been failed.", RequestType.Failed);
                    return this.BadRequest(new ErrorResponse { Message = "End date is less than start date." });
                }

                var managerGuid = Guid.Parse(this.UserAadId);
                var dashboardProjects = await this.managerDashboardHelper.GetDashboardProjectsAsync(managerGuid, startDate.Date, endDate.Date);

                this.RecordEvent("Get dashboard projects- The HTTP call to GET dashboard projects has been succeeded.", RequestType.Succeeded);
                return this.Ok(dashboardProjects);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get dashboard projects- The HTTP call to GET dashboard projects has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching projects details of dashboard.");
                throw;
            }
        }
    }
}
