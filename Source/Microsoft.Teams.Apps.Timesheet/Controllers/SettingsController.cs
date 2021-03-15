// <copyright file="SettingsController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Controllers
{
    using System;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Handles API request to get application settings.
    /// </summary>
    [Route("api/settings")]
    [ApiController]
    [Authorize]
    public class SettingsController : BaseController
    {
        /// <summary>
        /// Logs errors and information
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// A set of key/value application configuration properties.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="botOptions">A set of key/value application configuration properties.</param>
        public SettingsController(
            ILogger<ProjectController> logger,
            TelemetryClient telemetryClient,
            IOptions<BotSettings> botOptions)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.botOptions = botOptions;
        }

        /// <summary>
        /// Gets application settings required for client app validations.
        /// </summary>
        /// <returns>Returns application settings.</returns>
        [HttpGet]
        [ResponseCache(Duration = 86400)] // Cache for 1 day.
        public IActionResult GetValidationParameters()
        {
            this.RecordEvent("Get validation parameters- The HTTP GET call to get resources has been initiated.", RequestType.Initiated);

            try
            {
                var applicationSettings = new SettingsDTO
                {
                    TimesheetFreezeDayOfMonth = this.botOptions.Value.TimesheetFreezeDayOfMonth,
                    WeeklyEffortsLimit = this.botOptions.Value.WeeklyEffortsLimit,
                };

                this.RecordEvent("Get validation parameters- The HTTP GET call to get resources has been succeeded.", RequestType.Succeeded);

                return this.Ok(applicationSettings);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get validation parameters- The HTTP GET call to get resources has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching settings.");
                throw;
            }
        }
    }
}
