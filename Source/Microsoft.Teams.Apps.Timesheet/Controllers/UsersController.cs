// <copyright file="UsersController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using ErrorResponse = Microsoft.Teams.Apps.Timesheet.Models.ErrorResponse;

    /// <summary>
    /// User controller is responsible to expose API endpoints to fetch reportees and manager details.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UsersController : BaseController
    {
        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The instance of user Graph service to access logged-in user's reportees and manager.
        /// </summary>
        private readonly IUsersService userGraphService;

        /// <summary>
        /// Instance of timesheet helper which helps in managing operations timesheet entity.
        /// </summary>
        private readonly ITimesheetHelper timesheetHelper;

        /// <summary>
        /// The instance of user helper to fetch reportees of manager.
        /// </summary>
        private readonly IUserHelper userHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information.</param>
        /// <param name="userGraphService">The instance of user Graph service to access logged-in user's reportees and manager.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="timesheetHelper">Instance of project helper which helps in managing operations project entity.</param>
        /// <param name="userHelper">The instance of user helper.</param>
        public UsersController(
            ILogger<UsersController> logger,
            IUsersService userGraphService,
            TelemetryClient telemetryClient,
            ITimesheetHelper timesheetHelper,
            IUserHelper userHelper)
            : base(telemetryClient)
        {
            this.userGraphService = userGraphService;
            this.logger = logger;
            this.timesheetHelper = timesheetHelper;
            this.userHelper = userHelper;
        }

        /// <summary>
        /// Get user profiles by user object Ids.
        /// </summary>
        /// <param name="userIds">List of user object Ids.</param>
        /// <returns>List of users profile.</returns>
        [HttpPost]
        public async Task<IActionResult> GetUsersProfileAsync([FromBody] IEnumerable<string> userIds)
        {
            this.RecordEvent("Get users profiles- The HTTP call to GET users profiles has been initiated.", RequestType.Initiated);

            if (userIds.IsNullOrEmpty())
            {
                this.RecordEvent("Get users profiles- The HTTP call to GET users profiles has been failed.", RequestType.Failed);
                this.logger.LogError("User Id list cannot be null or empty.");
                return this.BadRequest(new { message = "User Id list cannot be null or empty." });
            }

            try
            {
                var userProfiles = await this.userGraphService.GetUsersAsync(userIds);
                this.RecordEvent("Get users profiles- The HTTP call to GET users profiles has been succeeded.", RequestType.Succeeded);

                if (userProfiles != null)
                {
                    return this.Ok(userProfiles.Select(user => new UserDTO { DisplayName = user.Value.DisplayName, Id = user.Value.Id }));
                }

                return this.NoContent();
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get users profiles- The HTTP call to GET users profiles has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching users profiles.");
                throw;
            }
        }

        /// <summary>
        /// Get direct reportees for logged-in user.
        /// </summary>
        /// <param name="search">Search text for querying over display name and email of user.</param>
        /// <returns>Returns list of users who report to logged-in user.</returns>
        [HttpGet("me/reportees")]
        public async Task<IActionResult> GetMyReporteesAsync([FromQuery] string search)
        {
            this.RecordEvent("Get reportees- The HTTP GET call to get reportees has been initiated.", RequestType.Initiated);

            try
            {
                var reporteesResponse = await this.userGraphService.GetMyReporteesAsync(search);

                this.RecordEvent("Get reportees- The HTTP GET call to get reportees has succeeded.", RequestType.Succeeded);
                var reportees = reporteesResponse.Select(user => new ReporteeDTO
                {
                    DisplayName = user.DisplayName,
                    Id = Guid.Parse(user.Id),
                    UserPrincipalName = user.UserPrincipalName,
                });

                return this.Ok(reportees);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get reportees- The HTTP GET call to get reportees has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching reportees.");
                throw;
            }
        }

        /// <summary>
        /// Get manager for logged-in user.
        /// </summary>
        /// <returns>Returns manager details.</returns>
        [HttpGet("me/manager")]
        public async Task<IActionResult> GetManagerAsync()
        {
            this.RecordEvent("Get manager- The HTTP GET call to get manager has been initiated.", RequestType.Initiated);

            try
            {
                var managerDetails = await this.userGraphService.GetManagerAsync();

                this.RecordEvent("Get manager- The HTTP GET call to get manager has succeeded.", RequestType.Succeeded);

                return this.Ok(managerDetails);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get manager- The HTTP GET call to get manager has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching manager details.");
                throw;
            }
        }

        /// <summary>
        /// Gets timesheets of user for given status.
        /// </summary>
        /// <param name="reporteeId">The user id of which timesheets to get.</param>
        /// <param name="status">Timesheet status (<see cref="TimesheetStatus"></see>) for filtering timesheets./></param>
        /// <returns>List of timesheets.</returns>
        [HttpGet("{reporteeId}/timesheets/{status}")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        public async Task<IActionResult> GetTimesheetsByStatusAsync(Guid reporteeId, int status)
        {
            this.RecordEvent("Get timesheet by status- The HTTP GET call has been initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "reporteeId", reporteeId.ToString() },
            });

            if (!Enum.IsDefined(typeof(TimesheetStatus), status))
            {
                this.logger.LogError("Invalid status.");
                this.RecordEvent("Get timesheets by status- The HTTP GET call has been failed.", RequestType.Failed);
                return this.BadRequest(new ErrorResponse { Message = "Invalid timesheet status." });
            }

            try
            {
                // Check if user reports to logged in manager.
                var reportees = await this.userGraphService.GetMyReporteesAsync(search: string.Empty);
                if (!reportees.Any(reportee => reportee.Id == reporteeId.ToString()))
                {
                    this.logger.LogError("Manager is not authorized to view timesheets of reportee.");
                    this.RecordEvent("Get timesheets by status- The HTTP GET call has been failed.", RequestType.Failed);
                    return this.Unauthorized(new ErrorResponse { Message = "Manager is not authorized to view timesheets of reportee." });
                }

                var timesheets = this.timesheetHelper.GetTimesheetsByStatus(reporteeId, (TimesheetStatus)status);
                this.RecordEvent("Get timesheets by status- The HTTP GET call has been succeeded.", RequestType.Succeeded);

                return this.Ok(timesheets);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get timesheets by status- The HTTP GET call has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching submitted timesheets.");
                throw;
            }
        }

        /// <summary>
        /// Gets all active projects along with tasks assigned to user between specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="reporteeId">The reportee Id of which projects to get.</param>
        /// <returns>Returns all active projects assigned to user on particular date range.</returns>
        [HttpGet("{reporteeId}/timesheets")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        public async Task<IActionResult> GetUserTimesheetsOverviewAsync(DateTime calendarStartDate, DateTime calendarEndDate, Guid reporteeId)
        {
            this.RecordEvent("Get projects- The HTTP call to GET projects details of user has been initiated.", RequestType.Initiated, new Dictionary<string, string>
            {
                { "reporteeId", reporteeId.ToString() },
            });

            if (calendarEndDate < calendarStartDate)
            {
                this.logger.LogError("Calendar end date is greater than start date.");
                this.RecordEvent("Get timesheets overview- The HTTP call to GET projects details of user has been failed.", RequestType.Failed);
                return this.BadRequest(new ErrorResponse { Message = "Calendar end date is greater than start date." });
            }

            try
            {
                // Get all reportees of logged-in manager.
                var reportees = await this.userHelper.GetAllReporteesAsync(managerObjectId: Guid.Parse(this.UserAadId));

                if (!reportees.Any(reportee => reportee.Id == reporteeId.ToString()))
                {
                    this.logger.LogError("Reportee does not report to logged-in manager.");
                    this.RecordEvent("Get timesheets overview- The HTTP call to GET projects details of user has been failed.", RequestType.Failed);
                    return this.Unauthorized(new ErrorResponse { Message = "Reportee does not reports to logged-in manager." });
                }

                var userTimesheets = await this.timesheetHelper.GetTimesheetsAsync(calendarStartDate, calendarEndDate, reporteeId);
                this.RecordEvent("Get timesheets overview- The HTTP call to GET projects details of user has been succeeded.", RequestType.Succeeded);

                return this.Ok(userTimesheets);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get timesheets overview- The HTTP call to GET projects details of user has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occured while fetching projects details of users.");
                throw;
            }
        }
    }
}
