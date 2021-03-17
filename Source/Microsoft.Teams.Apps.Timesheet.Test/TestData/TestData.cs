// <copyright file="TestData.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.TestData
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Models.Configuration;

    /// <summary>
    /// Provide test data to test methods.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// Submitted timesheet test list.
        /// </summary>
        public static readonly List<TimesheetEntity> SubmittedTimesheets = new List<TimesheetEntity>
        {
            new TimesheetEntity
            {
                Id = Guid.Parse("0a0a285f-7b97-45a8-82c3-58562b69a1ce"),
                TaskId = Guid.NewGuid(),
                Hours = 10,
                Status = (int)TimesheetStatus.Submitted,
                UserId = Guid.Parse("1a1a285f-7b97-45a8-82c3-58562b69a1ce"),
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 25),
                Task = new TaskEntity
                {
                    Project = new Project
                    {
                        Id = Guid.Parse("2a2a285f-7b97-45a8-82c3-58562b57a1ce"),
                        Title = "Project 1",
                    },
                },
            },
        };

        /// <summary>
        /// Request approval DTO test list.
        /// </summary>
        public static readonly List<RequestApprovalDTO> RequestApprovalDTOs = new List<RequestApprovalDTO>
        {
            new RequestApprovalDTO
            {
                ManagerComments = string.Empty,
                TimesheetDate = new List<DateTime>
                    {
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day),
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1),
                    },
                UserId = Guid.Parse("99051013-15d3-4831-a301-ded45bf3d32a"),
                TimesheetId = Guid.Parse("0a0a285f-7b97-45a8-82c3-58562b69a1ce"),
            },
        };

        /// <summary>
        /// List of members for testing.
        /// </summary>
        public static readonly IEnumerable<Member> InvalidMembers = new List<Member>
        {
            new Member
            {
                Id = Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a54e"),
                UserId = Guid.Parse("e5be1d47-2707-4dfc-b2a9-e62648c3a04e"),
                IsBillable = true,
                IsRemoved = false,
                ProjectId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
            },
        };

        /// <summary>
        /// Dashboard request DTO list.
        /// </summary>
        public static readonly List<DashboardRequestDTO> DashboardRequestDTOs = new List<DashboardRequestDTO>
        {
            new DashboardRequestDTO
            {
                UserId = Guid.NewGuid(),
            },
        };

        /// <summary>
        /// Test task model.
        /// </summary>
        public static readonly TaskEntity Task = new TaskEntity
        {
            StartDate = DateTime.Now.AddDays(-9),
            EndDate = DateTime.Now.AddDays(9),
            IsAddedByMember = true,
            MemberMappingId = Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a54e"),
            ProjectId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
            Title = "Test task",
            IsRemoved = false,
            MemberMapping = new Member
            {
                Id = Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a54e"),
                UserId = Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"),
                IsBillable = true,
                IsRemoved = false,
                ProjectId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
            },
        };

        /// <summary>
        /// Bot options to be used in test methods.
        /// </summary>
        public static readonly IOptions<BotSettings> BotOptions = Options.Create(new BotSettings()
        {
            MicrosoftAppId = "{Application id}",
            MicrosoftAppPassword = "{Application password or secret}",
            AppBaseUri = "https://2db43ef5248b.ngrok.io/",
            CardCacheDurationInHour = 1,
        });

        /// <summary>
        /// Azure settings to be used in test methods.
        /// </summary>
        public static readonly IOptions<AzureSettings> AzureSettings = Options.Create(new AzureSettings()
        {
            ClientId = "{Application id}",
        });

        /// <summary>
        /// Expected project DTO to be verify in test.
        /// </summary>
        public static readonly ProjectDTO ExpectedProjectDTO = new ProjectDTO
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

        /// <summary>
        /// Expected project tasks overview to be verify in test.
        /// </summary>
        public static readonly IEnumerable<ProjectTaskOverviewDTO> ExpectedProjectTasksOverview = new List<ProjectTaskOverviewDTO>
        {
            new ProjectTaskOverviewDTO
            {
                Id = Guid.Parse("82ab7412-f6c1-491d-be16-f797e6903123"),
                Title = "TaskEntity",
                TotalHours = 0,
                IsRemoved = false,
            },
        };

        /// <summary>
        /// Expected project members overview to be verify in test.
        /// </summary>
        public static readonly IEnumerable<ProjectMemberOverviewDTO> ExpectedProjectMembersOverview = new List<ProjectMemberOverviewDTO>
        {
            new ProjectMemberOverviewDTO
            {
                Id = Guid.Parse("54ab7412-f6c1-491d-be16-f797e6903667"),
                IsBillable = true,
                UserId = Guid.Parse("82ab7412-f6c1-491d-be16-f797e6903667"),
                UserName = "ABC",
                TotalHours = 0,
                IsRemoved = false,
            },
        };

        /// <summary>
        /// Expected project utilization to be verify in test.
        /// </summary>
        public static readonly ProjectUtilizationDTO ExpectedProjectUtilization = new ProjectUtilizationDTO
        {
            Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
            Title = "Project 1",
            BillableUnderutilizedHours = 2,
            BillableUtilizedHours = 10,
            NonBillableUtilizedHours = 10,
            NonBillableUnderutilizedHours = 2,
            TotalHours = 24,
        };

        /// <summary>
        /// Expected dashboard projects to be verify in test.
        /// </summary>
        public static readonly IEnumerable<DashboardProjectDTO> ExpectedDashboardProjects = new List<DashboardProjectDTO>
        {
            new DashboardProjectDTO
            {
                Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                Title = "Project 1",
                TotalHours = 60,
                UtilizedHours = 10,
            },
        };

        /// <summary>
        /// Project update DTO model.
        /// </summary>
        public static readonly ProjectUpdateDTO ProjectUpdateDTO = new ProjectUpdateDTO
        {
            BillableHours = 50,
            NonBillableHours = 10,
            ClientName = "Samuel,",
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month, 28),
            Id = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
            Title = "Project 1",
        };

        /// <summary>
        /// Approved timesheet test list.
        /// </summary>
        public static readonly IEnumerable<TimesheetEntity> ApprovedTimesheets = new List<TimesheetEntity>
        {
            new TimesheetEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                UserId = Guid.Parse("3fd7af65-67df-43cb-baa0-30917e133d94"),
                Status = (int)TimesheetStatus.Approved,
                Hours = 5,
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 4),
                Task = new TaskEntity
                {
                    ProjectId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
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
                Status = (int)TimesheetStatus.Approved,
                Hours = 5,
                TimesheetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 4),
                Task = new TaskEntity
                {
                    ProjectId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"),
                    Project = new Project
                    {
                        Title = "Project",
                    },
                },
            },
        };

        /// <summary>
        /// Members test list.
        /// </summary>
        public static readonly IEnumerable<Member> Members = new List<Member>
        {
            new Member
            {
                ProjectId = Guid.NewGuid(),
                UserId = Guid.Parse("82ab7412-f6c1-491d-be16-f797e6903667"),
                IsBillable = true,
                Id = Guid.Parse("54ab7412-f6c1-491d-be16-f797e6903667"),
            },
        };

        /// <summary>
        /// Members DTO test list.
        /// </summary>
        public static readonly List<MemberDTO> MembersDTO = new List<MemberDTO>
        {
            new MemberDTO
            {
                ProjectId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                IsBillable = true,
                Id = Guid.NewGuid(),
            },
        };

        /// <summary>
        /// Existing members test list.
        /// </summary>
        public static readonly List<MemberDTO> ExistingMembers = new List<MemberDTO>
        {
            new MemberDTO
            {
                ProjectId = Guid.NewGuid(),
                UserId = Guid.Parse("82ab7412-f6c1-491d-be16-f797e6903667"),
                IsBillable = false,
                Id = Guid.NewGuid(),
            },
        };

        /// <summary>
        /// Project members overview DTO test list.
        /// </summary>
        public static readonly List<ProjectMemberOverviewDTO> ProjectMemberOverviewDTOs = new List<ProjectMemberOverviewDTO>
        {
            new ProjectMemberOverviewDTO
            {
                Id = Guid.NewGuid(),
                IsBillable = true,
                IsRemoved = true,
                TotalHours = 5,
                UserId = Guid.Parse("82ab7412-f6c1-491d-be16-f797e6903667"),
                UserName = "Random",
            },
        };

        /// <summary>
        /// Tasks test list.
        /// </summary>
        public static readonly IEnumerable<TaskEntity> Tasks = new List<TaskEntity>
        {
            new TaskEntity
            {
                Id = Guid.Parse("82ab7412-f6c1-491d-be16-f797e6903123"),
                ProjectId = Guid.NewGuid(),
                Title = "TaskEntity",
                IsRemoved = false,
            },
        };

        /// <summary>
        /// Tasks DTO test list.
        /// </summary>
        public static readonly List<TaskDTO> TaskDTOs = new List<TaskDTO>
        {
            new TaskDTO
            {
                Id = Guid.Parse("82ab7412-f6c1-491d-be16-f797e6903123"),
                ProjectId = Guid.NewGuid(),
                Title = "TaskEntity",
            },
        };

        /// <summary>
        /// The project test data.
        /// </summary>
        public static readonly List<Project> Projects = new List<Project>()
        {
            new Project
            {
                Id = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                Title = "TimesheetEntity App",
                ClientName = "Microsoft",
                BillableHours = 200,
                NonBillableHours = 200,
                StartDate = new DateTime(DateTime.UtcNow.Year, 1, 2),
                EndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 28),
                Members = new List<Member>
                {
                    new Member
                    {
                        Id = Guid.Parse("d3d964ae-2979-4dac-b1e0-6c1b936c2640"),
                        ProjectId = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                        UserId = Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"),
                        IsBillable = true,
                        IsRemoved = false,
                    },
                },
                Tasks = new List<TaskEntity>
                {
                    new TaskEntity
                    {
                        Id = Guid.Parse("2dcf17b4-9bc7-488a-a59c-b0d12b14782d"),
                        ProjectId = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                        IsRemoved = false,
                        Title = "Development",
                    },
                },
                CreatedBy = Guid.Parse("08310120-ff64-45a4-b67a-6f2f19fba937"),
                CreatedOn = DateTime.Now,
            },
        };

        /// <summary>
        /// Reportees test list.
        /// </summary>
        public static readonly List<User> Reportees = new List<User>
        {
            new User
            {
                Id = "1ce072c1-1b87-4912-bb60-307698e6874e",
            },
        };
    }
}