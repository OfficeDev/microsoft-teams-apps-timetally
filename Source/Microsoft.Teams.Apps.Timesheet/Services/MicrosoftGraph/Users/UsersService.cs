// <copyright file="UsersService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;

    /// <summary>
    /// Users service to fetch reportees data from Microsoft Graph.
    /// </summary>
    internal class UsersService : IUsersService
    {
        /// <summary>
        /// MS Graph batch limit is 20
        /// https://docs.microsoft.com/en-us/graph/known-issues#json-batching.
        /// </summary>
        private const int BatchSplitCount = 20;

        /// <summary>
        /// Instance of Microsoft Graph service client.
        /// </summary>
        private readonly IGraphServiceClient graphServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersService"/> class.
        /// </summary>
        /// <param name="graphServiceClient">Microsoft Graph service client.</param>
        internal UsersService(IGraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
        }

        /// <summary>
        /// Get direct reportees for logged in user.
        /// If search text is provided then reportees will be filtered on the basis of display name and email.
        /// </summary>
        /// <param name="search">Text by which reportees will be filtered.</param>
        /// <returns>List of reportees.</returns>
        public async Task<IEnumerable<User>> GetMyReporteesAsync(string search)
        {
            var reportees = new List<User>();

            var directReportees = await this.graphServiceClient.Me.DirectReports.Request()
                .Select("id,displayName,userPrincipalName,mail").GetAsync();

            do
            {
                IEnumerable<DirectoryObject> searchedReportees = directReportees.CurrentPage;

                if (search != null && search.Length > 0)
                {
                    searchedReportees = directReportees.CurrentPage.Where(reportee => ((User)reportee).DisplayName.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                        || ((User)reportee).Mail.Contains(search, StringComparison.InvariantCultureIgnoreCase));
                }

                reportees.AddRange(searchedReportees.Cast<User>());

                // If there are more result.
                if (directReportees.NextPageRequest != null)
                {
                    directReportees = await directReportees.NextPageRequest.GetAsync();
                }
                else
                {
                    break;
                }
            }
            while (directReportees.CurrentPage != null);

            return reportees;
        }

        /// <summary>
        /// Get manager of logged in user.
        /// </summary>
        /// <returns>Directory object associated with manager.</returns>
        public async Task<DirectoryObject> GetManagerAsync()
        {
            return await this.graphServiceClient.Me.Manager.Request().GetAsync();
        }

        /// <summary>
        /// Get users information from graph API.
        /// </summary>
        /// <param name="userObjectIds">Collection of AAD Object ids of users.</param>
        /// <returns>Returns user id and details key value pairs.</returns>
        public async Task<Dictionary<Guid, User>> GetUsersAsync(IEnumerable<string> userObjectIds)
        {
            userObjectIds = userObjectIds ?? throw new ArgumentNullException(nameof(userObjectIds));
            var userDetails = new List<User>();
            var userObjectIdBatches = userObjectIds.ToList().SplitList(BatchSplitCount);

            BatchRequestContent batchRequestContent;
            foreach (var userObjectIdBatch in userObjectIdBatches)
            {
                var batchIds = new List<string>();
                var userDetailsBatch = new List<User>();
                using (batchRequestContent = new BatchRequestContent())
                {
                    foreach (string userObjectId in userObjectIdBatch)
                    {
                        var request = this.graphServiceClient
                            .Users[userObjectId]
                            .Request();

                        batchIds.Add(batchRequestContent.AddBatchRequestStep(request));
                    }

                    var response = await this.graphServiceClient.Batch.Request().PostAsync(batchRequestContent);
                    for (int i = 0; i < batchIds.Count; i++)
                    {
                        userDetailsBatch.Add(await response.GetResponseByIdAsync<User>(batchIds[i]));
                    }

                    userDetails.AddRange(userDetailsBatch);
                }
            }

            return userDetails.ToDictionary(user => Guid.Parse(user.Id), user => user);
        }
    }
}