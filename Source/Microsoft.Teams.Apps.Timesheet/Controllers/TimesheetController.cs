// <copyright file="TimesheetController.cs" company="Microsoft Corporation">
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
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Handles API requests related to timesheet.
    /// </summary>
    [Route("api/timesheets")]
    [ApiController]
    [Authorize]
    public class TimesheetController : BaseController
    {
        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Holds the instance of timesheet helper.
        /// </summary>
        private readonly ITimesheetHelper timesheetHelper;

        /// <summary>
        /// The instance of manager dashboard helper which helps in managing operations on dashboard entity.
        /// </summary>
        private readonly IManagerDashboardHelper managerDashboardHelper;

        /// <summary>
        /// Holds the instance of respository accessors for database operations.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimesheetController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="timesheetHelper">The instance of timesheet helper.</param>
        /// <param name="managerDashboardHelper">The instance of manager dashboard helper.</param>
        /// <param name="repositoryAccessors">Holds the instance of respository accessors for database operations.</param>
        public TimesheetController(
            ILogger<TimesheetController> logger,
            TelemetryClient telemetryClient,
            ITimesheetHelper timesheetHelper,
            IManagerDashboardHelper managerDashboardHelper,
            IRepositoryAccessors repositoryAccessors)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.timesheetHelper = timesheetHelper;
            this.managerDashboardHelper = managerDashboardHelper;
            this.repositoryAccessors = repositoryAccessors;
        }

        /// <summary>
        /// Handles API request to save timesheet of logged-in user.
        /// </summary>
        /// <param name="clientLocalCurrentDate">The client's local current date.</param>
        /// <param name="timesheetDetails">The timesheet details that needs to be saved.</param>
        /// <returns>Returns true if timesheet saved successfully. Else returns false.</returns>
        [HttpPost("{clientLocalCurrentDate}")]
        [Authorize(Policy = PolicyNames.MustBeProjectMemberPolicy)]
        public async Task<IActionResult> SaveTimesheetsAsync(DateTime clientLocalCurrentDate, [FromBody] IEnumerable<UserTimesheet> timesheetDetails)
        {
            this.RecordEvent("Save timesheet- The HTTP POST call to save timesheet has been initiated.", RequestType.Initiated);

            if (!this.timesheetHelper.IsClientCurrentDateValid(clientLocalCurrentDate, DateTime.UtcNow))
            {
                this.RecordEvent("Save timesheet- The HTTP POST call to save timesheet has been failed.", RequestType.Failed);
                return this.BadRequest(new ErrorResponse { Message = "The provided current date is invalid." });
            }

            try
            {
                var userTimesheetsToSave = timesheetDetails.Where(timesheet =>
                    timesheet.ProjectDetails != null && timesheet.ProjectDetails.Any(project => project.TimesheetDetails.Any()));

                // Filter timesheet dates those aren't frozen.
                var notYetFrozenTimesheetDates = this.timesheetHelper.GetNotYetFrozenTimesheetDates(userTimesheetsToSave.Select(x => x.TimesheetDate), DateTime.UtcNow.Date);

                userTimesheetsToSave = userTimesheetsToSave.Where(x => notYetFrozenTimesheetDates.Contains(x.TimesheetDate));

                if (userTimesheetsToSave.IsNullOrEmpty())
                {
                    this.logger.LogInformation("The timesheet can not be filled for frozen timesheet dates for user {0}.", this.UserAadId);
                    this.RecordEvent("Save timesheet- The HTTP POST call to save timesheet has been failed.", RequestType.Failed);
                    return this.BadRequest(new ErrorResponse { Message = "The timesheet can not be filled for frozen timesheet dates." });
                }

                var result = await this.timesheetHelper.SaveTimesheetsAsync(userTimesheetsToSave, clientLocalCurrentDate, Guid.Parse(this.UserAadId));

                if (result != null)
                {
                    this.RecordEvent("Save timesheet- The HTTP POST call to save timesheet has been succeeded.", RequestType.Succeeded);
                    return this.Ok(result);
                }

                this.RecordEvent("Save timesheet- The HTTP POST call to save timesheet has been failed.", RequestType.Failed);
                return this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to save timesheets.");
            }
            catch (Exception ex)
            {
                this.RecordEvent("Save timesheet- The HTTP POST call to save timesheet has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while saving timesheet.");
                throw;
            }
        }

        /// <summary>
        /// Handles API request to submit timesheet of logged-in user.
        /// </summary>
        /// <returns>Returns true if timesheet submitted successfully. Else returns false.</returns>
        [HttpPost("submit/")]
        [Authorize(Policy = PolicyNames.MustBeProjectMemberPolicy)]
        public async Task<IActionResult> SubmitTimesheetsAsync()
        {
            this.RecordEvent("Submit timesheet- The HTTP POST call to submit timesheet has been initiated.", RequestType.Initiated);

            try
            {
                var result = await this.timesheetHelper.SubmitTimesheetsAsync(Guid.Parse(this.UserAadId));

                if (result != null)
                {
                    this.RecordEvent("Submit timesheet- The HTTP POST call to submit timesheet has been succeeded.", RequestType.Succeeded);
                    return this.Ok(result);
                }

                this.RecordEvent("Submit timesheet- The HTTP POST call to submit timesheet has been failed.", RequestType.Failed);
                return this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to submit timesheets.");
            }
            catch (Exception ex)
            {
                this.RecordEvent("Submit timesheet- The HTTP POST call to submit timesheet has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while submitting timesheet.");
                throw;
            }
        }

        /// <summary>
        /// Duplicates the efforts of source date to target dates.
        /// </summary>
        /// <param name="clientLocalCurrentDate">The client's local current date.</param>
        /// <param name="duplicateEffortsDetails">The object containing information of dates used to duplicate efforts.</param>
        /// <returns>Return true if efforts duplicated successfully. Else returns false.</returns>
        [HttpPost("duplicate/{clientLocalCurrentDate}")]
        [Authorize(Policy = PolicyNames.MustBeProjectMemberPolicy)]
        public async Task<IActionResult> DuplicateEffortsAsync(DateTime clientLocalCurrentDate, [FromBody] DuplicateEffortsDTO duplicateEffortsDetails)
        {
            this.RecordEvent("Duplicate efforts- The HTTP POST call to duplicate efforts has been initiated.", RequestType.Initiated);

            // Validates whether client provided local date is valid.
            if (!this.timesheetHelper.IsClientCurrentDateValid(clientLocalCurrentDate, DateTime.UtcNow))
            {
                this.logger.LogInformation("The timesheet can not be filled as provided current date is invalid.");
                this.RecordEvent("Duplicate efforts- The HTTP POST call to duplicate efforts has been failed.", RequestType.Failed);
                return this.BadRequest(new ErrorResponse { Message = "The timesheet can not be filled as provided current date is invalid." });
            }

            try
            {
                // Get target dates those aren't frozen.
#pragma warning disable CA1062 // Model validation is done by data annotations.
                var notYetFrozenTimesheetDates = this.timesheetHelper.GetNotYetFrozenTimesheetDates(duplicateEffortsDetails.TargetDates, clientLocalCurrentDate);
#pragma warning restore CA1062 // Model validation is done by data annotations.

                // If all target dates are frozen.
                if (notYetFrozenTimesheetDates.IsNullOrEmpty())
                {
                    this.logger.LogInformation("The timesheet can not be filled for frozen timesheet dates for user {0}.", this.UserAadId);
                    this.RecordEvent("Duplicate efforts- The HTTP POST call to duplicate efforts has been failed.", RequestType.Failed);
                    return this.BadRequest(new ErrorResponse { Message = "The timesheet can not be filled as the target dates are frozen." });
                }

                var timesheets = await this.timesheetHelper.GetTimesheetsAsync(duplicateEffortsDetails.SourceDate, duplicateEffortsDetails.SourceDate, Guid.Parse(this.UserAadId));
                var sourceDateTimesheet = timesheets.FirstOrDefault();

                if (sourceDateTimesheet == null || sourceDateTimesheet.ProjectDetails.IsNullOrEmpty())
                {
                    this.logger.LogInformation("The source date must have projects.");
                    this.RecordEvent("Duplicate efforts- The HTTP POST call to duplicate efforts has been failed.", RequestType.Failed);
                    return this.BadRequest(new ErrorResponse { Message = "The source date must have projects." });
                }

#pragma warning disable CA1062 // Validated arguments at model level
                var result = await this.timesheetHelper.DuplicateEffortsAsync(duplicateEffortsDetails.SourceDate, duplicateEffortsDetails.TargetDates, clientLocalCurrentDate, Guid.Parse(this.UserAadId));
#pragma warning restore CA1062 // Validated arguments at model level

                if (result != null)
                {
                    this.RecordEvent("Duplicate efforts- The HTTP POST call to duplicate efforts has been succeeded.", RequestType.Succeeded);
                    return this.Ok(result);
                }

                this.RecordEvent("Duplicate efforts- The HTTP POST call to duplicate efforts has been failed.", RequestType.Failed);
                return this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to duplicate efforts.");
            }
            catch (Exception ex)
            {
                this.RecordEvent("Duplicate efforts- The HTTP POST call to duplicate efforts has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while duplicating efforts.");
                throw;
            }
        }

        /// <summary>
        /// Handles API request to get timesheets of logged-in user within specific date range.
        /// </summary>
        /// <param name="startDate">The start date from which timesheets needs to be retrieved.</param>
        /// <param name="endDate">The end date up to which timesheets needs to be retrieved.</param>
        /// <returns>Returns the timesheets assigned to logged-in user.</returns>
        [HttpGet]
        [Authorize(Policy = PolicyNames.MustBeProjectMemberPolicy)]
        public async Task<IActionResult> GetTimesheetsAsync(DateTime startDate, DateTime endDate)
        {
            this.RecordEvent("Get timesheets- The HTTP GET call to get timesheets has been initiated.", RequestType.Initiated);

            if (startDate.Date > endDate.Date)
            {
                this.logger.LogError("The provided start date is greater than end date.");

                this.RecordEvent("Get timesheets- The HTTP GET call to get timesheets has been failed.", RequestType.Failed, new Dictionary<string, string>
                {
                    { "startDate", startDate.ToString("O", CultureInfo.InvariantCulture) },
                    { "endDate", endDate.ToString("O", CultureInfo.InvariantCulture) },
                });

                return this.BadRequest("The start date must be less than or equal to end date.");
            }

            try
            {
                var timesheets = await this.timesheetHelper.GetTimesheetsAsync(startDate.Date, endDate.Date, Guid.Parse(this.UserAadId));

                this.RecordEvent("Get timesheets- The HTTP GET call to get timesheets has been succeeded.", RequestType.Succeeded);

                return this.Ok(timesheets);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get timesheets- The HTTP GET call to get timesheets has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching timesheets.");
                throw;
            }
        }

        /// <summary>
        /// Approve pending timesheets of a user for multiple dates.
        /// </summary>
        /// <param name="timesheetsToApprove">Holds the details of user and timesheet to approve.</param>
        /// <returns>Returns true if requests are updated, else return false.</returns>
        [HttpPost("approve")]
        [Authorize(PolicyNames.MustBeManagerOfReporteePolicy)]
        public async Task<IActionResult> ApproveTimesheetsAsync([FromBody] IEnumerable<RequestApprovalDTO> timesheetsToApprove)
        {
            this.RecordEvent("Approve timesheets- The HTTP POST call to approve timesheets has been initiated.", RequestType.Initiated);

            if (timesheetsToApprove.IsNullOrEmpty())
            {
                this.logger.LogError("Timesheets are either null or empty.");
                this.RecordEvent("Approve timesheets- The HTTP POST call to approve timesheets has been failed.", RequestType.Failed);
                return this.BadRequest(new ErrorResponse { Message = "Timesheets to update is null." });
            }

            try
            {
                var timesheetIds = timesheetsToApprove.Select(timesheetsToApprove => timesheetsToApprove.TimesheetId);

                // Validate if all timesheet has status 'Submitted' and saved against project which has been created by logged in manager.
                var submittedTimesheets = this.timesheetHelper.GetSubmittedTimesheetsByIds(Guid.Parse(this.UserAadId), timesheetIds);

                if (submittedTimesheets == null)
                {
                    this.RecordEvent("Approve timesheets- The HTTP POST call to approve timesheets has failed.", RequestType.Failed);
                    this.logger.LogInformation("No submitted timesheets found for user {0}.", this.UserAadId);
                    return this.NotFound("Timesheets not found.");
                }

                var approvalResponse = await this.timesheetHelper.ApproveOrRejectTimesheetsAsync(submittedTimesheets, timesheetsToApprove, TimesheetStatus.Approved);
                if (approvalResponse)
                {
                    this.RecordEvent("Approve timesheets- The HTTP POST call to approve timesheets has been succeeded.", RequestType.Succeeded);
                    return this.StatusCode((int)HttpStatusCode.NoContent);
                }

                this.RecordEvent("Approve timesheets- The HTTP POST call to approve timesheets has been failed.", RequestType.Failed);
                this.logger.LogError("Unable to update timesheets.");
                return this.StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse { Message = "Unable to update timesheets." });
            }
            catch (Exception ex)
            {
                this.RecordEvent("Approve timesheets- The HTTP POST call to approve timesheets has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while approving timesheets.");
                throw;
            }
        }

        /// <summary>
        /// Reject pending timesheets of a user for multiple dates.
        /// </summary>
        /// <param name="timesheetsToReject">Holds the details of user and timesheet dates to reject.</param>
        /// <returns>Returns true if timesheets are rejected, else return false.</returns>
        [HttpPost("reject")]
        [Authorize(PolicyNames.MustBeManagerOfReporteePolicy)]
        public async Task<IActionResult> RejectTimesheetsAsync([FromBody] IEnumerable<RequestApprovalDTO> timesheetsToReject)
        {
            this.RecordEvent("Reject timesheets- The HTTP POST call to reject timesheets has been initiated.", RequestType.Initiated);

            if (timesheetsToReject.IsNullOrEmpty())
            {
                this.logger.LogError("Timesheets list to reject is either null or empty.");
                this.RecordEvent("Reject timesheet- The HTTP POST call to reject timesheets has been failed.", RequestType.Failed);
                return this.BadRequest(new ErrorResponse { Message = "Timesheets list to reject is either null or empty." });
            }

            try
            {
                var timesheetIds = timesheetsToReject.Select(timesheetsToApprove => timesheetsToApprove.TimesheetId);

                // Validate if all timesheets has status 'Submitted' and saved against project which has been created by logged in manager.
                var submittedTimesheets = this.timesheetHelper.GetSubmittedTimesheetsByIds(Guid.Parse(this.UserAadId), timesheetIds);

                if (submittedTimesheets == null)
                {
                    this.RecordEvent("Reject timesheets- The HTTP POST call to reject timesheets has failed.", RequestType.Failed);
                    this.logger.LogInformation("No submitted timesheets found for user {0}.", this.UserAadId);
                    return this.NotFound("Timesheets not found.");
                }

                var rejectionResponse = await this.timesheetHelper.ApproveOrRejectTimesheetsAsync(submittedTimesheets, timesheetsToReject, TimesheetStatus.Rejected);

                if (rejectionResponse)
                {
                    this.RecordEvent("Reject timesheets- The HTTP POST call to reject timesheets has been succeeded.", RequestType.Succeeded);
                    return this.StatusCode((int)HttpStatusCode.NoContent);
                }

                this.RecordEvent("Reject timesheets- The HTTP POST call to reject timesheets has been failed.", RequestType.Failed);
                this.logger.LogError("Unable to update timesheets.");
                return this.StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse { Message = "Unable to update timesheets." });
            }
            catch (Exception ex)
            {
                this.RecordEvent("Reject timesheets- The HTTP POST call to reject timesheets has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while rejecting timesheets.");
                throw;
            }
        }

        /// <summary>
        /// Gets timesheets which are pending for manager approval.
        /// </summary>
        /// <returns>List of submitted timesheets.</returns>
        [HttpGet("dashboard")]
        [Authorize(PolicyNames.MustBeManagerPolicy)]
        public async Task<IActionResult> GetDashboardRequestsAsync()
        {
            this.RecordEvent("Get dashboard requests- The HTTP call to GET dashboard requests has been initiated.", RequestType.Initiated);
            try
            {
                var dashboardTimesheets = await this.managerDashboardHelper.GetDashboardRequestsAsync(Guid.Parse(this.UserAadId));

                this.RecordEvent("Get dashboard requests- The HTTP call to GET dashboard requests has been succeeded.", RequestType.Succeeded);
                return this.Ok(dashboardTimesheets);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get dashboard requests- The HTTP call to GET dashboard requests has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching dashboard requests.");
                throw;
            }
        }
    }
}