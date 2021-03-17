// <copyright file="UserHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;

    /// <summary>
    /// Provides helper methods for fetching reportees.
    /// </summary>
    public class UserHelper : IUserHelper
    {
        /// <summary>
        /// Holds the instance of Graph service to access logged in user's reportees and manager.
        /// </summary>
        private readonly IUsersService userGraphService;

        /// <summary>
        /// Instance of memory cache to cache reportees for managers.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// A set of key/value application configuration properties.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserHelper"/> class.
        /// </summary>
        /// <param name="userGraphService">The instance of user Graph service to access logged in user's reportees and manager.</param>
        /// <param name="memoryCache">Instance of memory cache to cache reportees for managers.</param>
        /// <param name="botOptions">A set of key/value application configuration properties.</param>
        public UserHelper(IUsersService userGraphService, IMemoryCache memoryCache,  IOptions<BotSettings> botOptions)
        {
            this.botOptions = botOptions;
            this.userGraphService = userGraphService;
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// Get direct reportees for logged in user.
        /// </summary>
        /// <param name="managerObjectId">Logged-in manager object Id.</param>
        /// <returns>List of reportees.</returns>
        public async Task<IEnumerable<User>> GetAllReporteesAsync(Guid managerObjectId)
        {
            var cacheKey = $"reportees_{managerObjectId}";
            this.memoryCache.TryGetValue(cacheKey, out IEnumerable<User> reportees);
            if (reportees.IsNullOrEmpty())
            {
                reportees = await this.userGraphService.GetMyReporteesAsync(search: string.Empty);
                this.memoryCache.Set(cacheKey, reportees, TimeSpan.FromHours(this.botOptions.Value.ManagerReporteesCacheDurationInHours));
            }

            return reportees;
        }

        /// <summary>
        /// Check if members are direct reportee of manager.
        /// </summary>
        /// <param name="memberIds">Ids of member.</param>
        /// <returns>Return true if members are direct reportee, else false.</returns>
        public async Task<bool> AreProjectMembersDirectReporteeAsync(IEnumerable<Guid> memberIds)
        {
            var allReportees = await this.userGraphService.GetMyReporteesAsync(search: string.Empty);
            var allReporteesIds = allReportees.Select(reportee => Guid.Parse(reportee.Id));

            // Check if added project members are direct reportees of logged-in manager.
            if (memberIds.All(memberId => allReporteesIds.Contains(memberId)))
            {
                return true;
            }

            return false;
        }
    }
}