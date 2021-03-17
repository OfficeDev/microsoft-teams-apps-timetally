// <copyright file="ManagerDashboardMapperTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Manager dashboard mapper tests contains test cases for getting distinct dates.
    /// </summary>
    [TestClass]
    public class ManagerDashboardMapperTests
    {
        /// <summary>
        /// Holds the instance of manager dashboard mapper.
        /// </summary>
        private ManagerDashboardMapper managerDashboardMapper;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.managerDashboardMapper = new ManagerDashboardMapper();
        }

        /// <summary>
        /// Test whether valid data is return with valid parameters while getting distinct dates.
        /// </summary>
        [TestMethod]
        public void GetDistinctDates_WithValidParams_ShouldReturnValidDates()
        {
            // ARRANGE
            var savedTimesheets = new List<TimesheetEntity>
            {
                new TimesheetEntity
                {
                    Id = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    Status = (int)TimesheetStatus.Saved,
                    Hours = 5,
                    TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                    Task = new TaskEntity
                    {
                        ProjectId = Guid.NewGuid(),
                        Project = new Project
                        {
                            Title = "Project",
                        },
                    },
                },
                new TimesheetEntity
                {
                    Id = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    Status = (int)TimesheetStatus.Saved,
                    Hours = 5,
                    TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                    Task = new TaskEntity
                    {
                        ProjectId = Guid.NewGuid(),
                        Project = new Project
                        {
                            Title = "Project",
                        },
                    },
                },
                new TimesheetEntity
                {
                    Id = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    Status = (int)TimesheetStatus.Saved,
                    Hours = 5,
                    TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 6),
                    Task = new TaskEntity
                    {
                        ProjectId = Guid.NewGuid(),
                        Project = new Project
                        {
                            Title = "Project",
                        },
                    },
                },
                new TimesheetEntity
                {
                    Id = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    Status = (int)TimesheetStatus.Saved,
                    Hours = 5,
                    TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 8),
                    Task = new TaskEntity
                    {
                        ProjectId = Guid.NewGuid(),
                        Project = new Project
                        {
                            Title = "Project",
                        },
                    },
                },
            };
            var expectedDateRange = new List<List<DateTime>>
            {
                new List<DateTime>
                {
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5).Date,
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, 6).Date,
                },
                new List<DateTime>
                {
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, 8).Date,
                },
            };

            // ACT
            var dateRange = this.managerDashboardMapper.GetGroupedDatesBySequence(savedTimesheets);

            // ASSERT
            Assert.IsFalse(dateRange.IsNullOrEmpty());

            // Check whether dates are grouped in expected number of lists.
            Assert.AreEqual(expectedDateRange.Count, dateRange.Count);

            for (int i = 0; i < expectedDateRange.Count; i++)
            {
                var expectedDates = expectedDateRange[i];
                var actualDates = dateRange[i];

                Assert.AreEqual(expectedDates.Count, actualDates.Count);

                // Verify dates in list.
                Assert.AreEqual(expectedDates.Count, actualDates.Intersect(expectedDates).Count());
            }
        }

        /// <summary>
        /// Test whether empty date range list is returned when no timesheets are passed.
        /// </summary>
        [TestMethod]
        public void GetDistinctDates_WithEmptyTimesheets_ShouldReturnEmptyList()
        {
            // ACT
            var dateRange = this.managerDashboardMapper.GetGroupedDatesBySequence(new List<TimesheetEntity>());

            // ASSERT
            Assert.IsTrue(!dateRange.Any());
        }

        /// <summary>
        /// Test whether valid view model is return with valid parameters.
        /// </summary>
        [TestMethod]
        public void MapForViewModel_WithValidParams_ShouldReturnValidViewModel()
        {
            // ARRANGE
            var timesheetsCollection = new List<List<TimesheetEntity>>
            {
                new List<TimesheetEntity>
                {
                    new TimesheetEntity
                    {
                        Id = Guid.Parse("4fd7af65-67df-43cb-baa0-30917e133d94"),
                        TaskId = Guid.NewGuid(),
                        UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                        Status = (int)TimesheetStatus.Saved,
                        Hours = 5,
                        TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                        Task = new TaskEntity
                        {
                            ProjectId = Guid.NewGuid(),
                            Project = new Project
                            {
                                Title = "Project",
                            },
                        },
                    },
                    new TimesheetEntity
                    {
                        Id = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e134d94"),
                        TaskId = Guid.NewGuid(),
                        UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                        Status = (int)TimesheetStatus.Saved,
                        Hours = 6,
                        TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                        Task = new TaskEntity
                        {
                            ProjectId = Guid.NewGuid(),
                            Project = new Project
                            {
                                Title = "Project",
                            },
                        },
                    },
                },
            };
            var expectedTimesheetViewModel = new List<DashboardRequestDTO>
            {
                new DashboardRequestDTO
                {
                    NumberOfDays = 1,
                    RequestedForDates = new List<List<DateTime>>
                    {
                        new List<DateTime>
                        {
                            new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                        },
                    },
                    Status = (int)TimesheetStatus.Saved,
                    SubmittedTimesheetIds = new List<Guid>
                    {
                        Guid.Parse("4fd7af65-67df-43cb-baa0-30917e133d94"),
                        Guid.Parse("3fd7af65-67df-43cb-baa0-30917e134d94"),
                    },
                    TotalHours = 11,
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    UserName = string.Empty,
                },
            };

            // ACT
            var viewModel = this.managerDashboardMapper.MapForViewModel(timesheetsCollection);

            // ASSERT
            Assert.AreEqual(expectedTimesheetViewModel.First().NumberOfDays, viewModel.ToList().First().NumberOfDays);
            Assert.AreEqual(expectedTimesheetViewModel.First().RequestedForDates.Count, viewModel.ToList().First().RequestedForDates.Count);
            Assert.AreEqual(expectedTimesheetViewModel.First().Status, viewModel.ToList().First().Status);
            Assert.AreEqual(expectedTimesheetViewModel.First().SubmittedTimesheetIds.Count(), viewModel.ToList().First().SubmittedTimesheetIds.Count());
            Assert.AreEqual(expectedTimesheetViewModel.First().TotalHours, viewModel.ToList().First().TotalHours);
            Assert.AreEqual(expectedTimesheetViewModel.First().UserId, viewModel.ToList().First().UserId);
        }
    }
}