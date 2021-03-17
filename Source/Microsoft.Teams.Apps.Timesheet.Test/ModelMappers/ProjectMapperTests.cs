// <copyright file="ProjectMapperTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Project mapper tests contains test cases for generating view models and create models.
    /// </summary>
    [TestClass]
    public class ProjectMapperTests
    {
        /// <summary>
        /// Holds the instance of project mapper.
        /// </summary>
        private ProjectMapper projectMapper;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.projectMapper = new ProjectMapper(new Mock<ILogger<ProjectMapper>>().Object);
        }

        /// <summary>
        /// Test whether valid create model is returned with valid view model.
        /// </summary>
        [TestMethod]
        public void MapForCreateModel_WithValidParams_ShouldReturnValidModel()
        {
            // ARRANGE
            var projectViewModel = new ProjectDTO
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
                        UserId = Guid.NewGuid(),
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

            var expectedCreateModel = new Project
            {
                BillableHours = projectViewModel.BillableHours,
                ClientName = projectViewModel.ClientName,
                EndDate = projectViewModel.EndDate,
                NonBillableHours = projectViewModel.NonBillableHours,
                StartDate = projectViewModel.StartDate,
                CreatedBy = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                CreatedOn = DateTime.UtcNow,
                Title = projectViewModel.Title,
                Members = projectViewModel.Members.IsNullOrEmpty() ? new List<Member>() :
                    projectViewModel.Members.Select(member => new Member
                    {
                        IsBillable = member.IsBillable,
                        UserId = member.UserId,
                        IsRemoved = false,
                    }).ToList(),
                Tasks = projectViewModel.Tasks.IsNullOrEmpty() ? new List<TaskEntity>() :
                    projectViewModel.Tasks.Select(task => new TaskEntity
                    {
                        Title = task.Title,
                        IsRemoved = false,
                        StartDate = task.StartDate.Date,
                        EndDate = task.EndDate.Date,
                    }).ToList(),
            };

            // ACT
            var resultCreateModel = this.projectMapper.MapForCreateModel(projectViewModel, Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"));

            // ASSERT
            Assert.AreEqual(expectedCreateModel.Members.Count(), resultCreateModel.Members.Count());
            Assert.AreEqual(expectedCreateModel.Tasks.Count(), resultCreateModel.Tasks.Count());
            Assert.IsFalse(resultCreateModel.Tasks.First().IsRemoved);
            Assert.IsFalse(resultCreateModel.Members.First().IsRemoved);
        }

        /// <summary>
        /// Test whether valid project utilization model is returned with valid parameters.
        /// </summary>
        [TestMethod]
        public void MapForProjectUtilizationViewModel_WithValidParams_ShouldReturnValidModel()
        {
            // ARRANGE
            var project = new Project
            {
                Id = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                Title = "TimesheetEntity App",
                ClientName = "Microsoft",
                BillableHours = 200,
                NonBillableHours = 200,
                StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 2),
                EndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 28),
                CreatedBy = Guid.Parse("08310120-ff64-45a4-b67a-6f2f19fba937"),
                CreatedOn = DateTime.Now,
            };

            var members = new List<Member>
            {
                new Member
                {
                    Id = Guid.Parse("d3d964ae-2979-4dac-b1e0-6c1b936c2640"),
                    ProjectId = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                    UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                    IsBillable = true,
                    IsRemoved = false,
                },
                new Member
                {
                    Id = Guid.Parse("d3d764ae-2979-4dac-b1e0-6c1b936c2640"),
                    ProjectId = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                    UserId = Guid.Parse("3fd8af65-67df-43cb-baa0-30917e133d94"),
                    IsBillable = true,
                    IsRemoved = false,
                },
            };

            var expectedProjectUtilization = new ProjectUtilizationDTO
            {
                Id = project.Id,
                Title = project.Title,
                BillableUtilizedHours = 10,
                NonBillableUtilizedHours = 0,
                BillableUnderutilizedHours = project.BillableHours - 10,
                NonBillableUnderutilizedHours = project.NonBillableHours - 0,
                TotalHours = project.BillableHours + project.NonBillableHours,
            };

            // ACT
            var resultUtilizationModel = this.projectMapper.MapForProjectUtilizationViewModel(project, TestData.TestData.ApprovedTimesheets, members);

            // ASSERT
            Assert.AreEqual(expectedProjectUtilization.BillableUtilizedHours, resultUtilizationModel.BillableUtilizedHours);
            Assert.AreEqual(expectedProjectUtilization.NonBillableUtilizedHours, resultUtilizationModel.NonBillableUtilizedHours);
            Assert.AreEqual(expectedProjectUtilization.BillableUnderutilizedHours, resultUtilizationModel.BillableUnderutilizedHours);
            Assert.AreEqual(expectedProjectUtilization.NonBillableUnderutilizedHours, resultUtilizationModel.NonBillableUnderutilizedHours);
            Assert.AreEqual(expectedProjectUtilization.TotalHours, resultUtilizationModel.TotalHours);
        }

        /// <summary>
        /// Test whether null is returned when user is not part of project.
        /// </summary>
        [TestMethod]
        public void MapForProjectUtilizationViewModel_WithInvalidMember_ShouldReturnNull()
        {
            // ARRANGE
            var project = new Project
            {
                Id = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                Title = "TimesheetEntity App",
                ClientName = "Microsoft",
                BillableHours = 200,
                NonBillableHours = 200,
                StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 2),
                EndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 28),
                CreatedBy = Guid.Parse("08310120-ff64-45a4-b67a-6f2f19fba937"),
                CreatedOn = DateTime.Now,
            };

            var members = new List<Member>
            {
                new Member
                {
                    Id = Guid.Parse("d3d764ae-2979-4dac-b1e0-6c1b936c2640"),
                    ProjectId = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                    UserId = Guid.Parse("3fd8af65-67df-43cb-baa0-30917e133d94"),
                    IsBillable = true,
                    IsRemoved = false,
                },
            };

            // ACT
            var resultUtilizationModel = this.projectMapper.MapForProjectUtilizationViewModel(project, TestData.TestData.ApprovedTimesheets, members);

            // ASSERT
            Assert.AreEqual(null, resultUtilizationModel);
        }
    }
}