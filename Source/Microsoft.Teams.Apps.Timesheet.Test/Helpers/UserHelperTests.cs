// <copyright file="UserHelperTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// User helper contains all the test cases.
    /// </summary>
    [TestClass]
    public class UserHelperTests
    {
        /// <summary>
        /// Holds the instance of user helper.
        /// </summary>
        private UserHelper userHelper;

        /// <summary>
        /// The mocked instance of user service.
        /// </summary>
        private Mock<IUsersService> userService;

        private Mock<IMemoryCache> memoryCache;

        private IOptions<BotSettings> botSettings;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.userService = new Mock<IUsersService>();
            this.memoryCache = new Mock<IMemoryCache>();
            this.botSettings = Options.Create(new BotSettings()
            {
                MicrosoftAppId = string.Empty,
                MicrosoftAppPassword = string.Empty,
                AppBaseUri = string.Empty,
                CardCacheDurationInHour = 12,
                TimesheetFreezeDayOfMonth = 12,
                WeeklyEffortsLimit = 44,
            });
            this.userHelper = new UserHelper(this.userService.Object, this.memoryCache.Object, this.botSettings);
        }

        /// <summary>
        /// Test whether true is return with valid reportee while checking members are direct reportee of logged-in manager.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AreProjectMembersDirectReportee_WithValidReportee_ShouldReturnTrue()
        {
            // ARRANGE
            // Test data of reportees.
            var reportees = new List<User>
            {
                new User
                {
                    Id = "975db6f8-181a-49c2-b5bc-ac4978b75bf1",
                },
                new User
                {
                    Id = "875db6f8-181a-49c2-b5bc-ac4978b75bf1",
                },
                new User
                {
                    Id = "675db6f8-181a-49c2-b5bc-ac4978b75bf1",
                },
            };

            // Mocked graph call to get reportee with test data.
            this.userService
                .Setup(service => service.GetMyReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(reportees.AsEnumerable()));

            // Member Ids of member to check if they are direct reportee.
            var memberIds = new List<Guid>
            {
                Guid.Parse("975db6f8-181a-49c2-b5bc-ac4978b75bf1"),
                Guid.Parse("875db6f8-181a-49c2-b5bc-ac4978b75bf1"),
            };

            // ACT
            var result = await this.userHelper.AreProjectMembersDirectReporteeAsync(memberIds);

            // ASSERT
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test whether false is return with invalid reportee while checking members are direct reportee of logged-in manager.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AreProjectMembersDirectReportee_WithInvalidValidReportee_ShouldReturnFalse()
        {
            // ARRANGE
            // Test data of reportees.
            var reportees = new List<User>
            {
                new User
                {
                    Id = "975db6f8-181a-49c2-b5bc-ac4978b75bf1",
                },
                new User
                {
                    Id = "875db6f8-181a-49c2-b5bc-ac4978b75bf1",
                },
                new User
                {
                    Id = "775db6f8-181a-49c2-b5bc-ac4978b75bf1",
                },
            };

            // Mocked graph call to get reportee with test data.
            this.userService
                .Setup(service => service.GetMyReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(reportees.AsEnumerable()));

            // Member Ids of member to check if they are direct reportee.
            var memberIds = new List<Guid>
            {
                Guid.Parse("675db6f8-181a-49c2-b5bc-ac4978b75bf1"),
                Guid.Parse("875db6f8-181a-49c2-b5bc-ac4978b75bf1"),
            };

            // ACT
            var result = await this.userHelper.AreProjectMembersDirectReporteeAsync(memberIds);

            // ASSERT
            Assert.IsFalse(result);
        }
    }
}