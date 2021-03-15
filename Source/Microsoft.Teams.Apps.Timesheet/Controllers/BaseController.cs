// <copyright file="BaseController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Base controller to handle API operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Instance of application insights telemetry client.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        public BaseController(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Gets the user Azure Active Directory id from the HttpContext.
        /// </summary>
        protected string UserAadId
        {
            get
            {
                var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
                var claim = this.User.Claims.FirstOrDefault(p => oidClaimType.Equals(p.Type, StringComparison.OrdinalIgnoreCase));
                if (claim == null)
                {
                    return null;
                }

                return claim.Value;
            }
        }

        /// <summary>
        /// Records event data to Application Insights telemetry client.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="requestStatus">The status of a request which is being processed.</param>
        /// <param name="payload">Payload which needs to be logged against event.</param>
        public void RecordEvent(string eventName, RequestType requestStatus, IDictionary<string, string> payload = null)
        {
            var payloadDictionary = new Dictionary<string, string>
            {
                { "userId", this.UserAadId },
                { "requestType", Enum.GetName(typeof(RequestType), requestStatus) },
            };

            if (payload != null)
            {
                foreach (var item in payload)
                {
                    payloadDictionary.Add(item.Key, item.Value);
                }
            }

            this.telemetryClient.TrackEvent(eventName, payloadDictionary);
        }
    }
}