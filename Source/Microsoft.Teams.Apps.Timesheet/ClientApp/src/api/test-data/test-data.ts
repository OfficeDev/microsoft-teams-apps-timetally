// <copyright file="test-data.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import moment from "moment";
import { Guid } from "guid-typescript";
import IUserTimesheet from "../../models/fill-timesheet/user-timesheet";
import { TimesheetStatus } from "../../models/timesheet-status";
import IResource from "../../models/resource";
import { IDashboardRequest } from "../../models/dashboard-request";
import { ISubmittedRequest } from "../../models/submitted-request";
import IUserSearchResult from "../../models/user-search-result";

const startOfCurrentWeek: Date = moment().startOf('week').startOf('day').toDate();
const projectEndDate: Date = moment(startOfCurrentWeek).add(10, 'day').startOf('day').toDate();

export default class TestData {

    public static getReportees: IUserSearchResult[] = [{
        displayName: "Cameron",
        id: "8d5f9c58-7738-4645-a3d9-e743a9e9f3e1",
        userPrincipalName: "cameronS@contoso.com"
    }]

    public static getDashboardRequests: IDashboardRequest[] = [{
        isSelected: false,
        numberOfDays: 1,
        requestedForDates: [[new Date()]],
        submittedTimesheetIds: [Guid.create()],
        totalHours: 10,
        userId: Guid.create(),
        userName: "Cameron",
        status: TimesheetStatus.Submitted
    }];

    public static getSubmittedRequests: ISubmittedRequest[] = [
        {
            isSelected: false,
            projectTitles: ["Project 1", "Project 2", "Project 3"],
            timesheetDate: new Date(),
            totalHours: 5,
            userId: Guid.parse("8d5f9c58-7738-4645-a3d9-e743a9e9f3e1"),
            status: TimesheetStatus.Submitted,
            submittedTimesheetIds: [Guid.create(), Guid.create()],
        },
        {
            isSelected: false,
            projectTitles: ["Project 1"],
            timesheetDate: new Date(),
            totalHours: 5,
            userId: Guid.parse("8d5f9c58-7738-4645-a3d9-e743a9e9f3e1"),
            status: TimesheetStatus.Submitted,
            submittedTimesheetIds: [Guid.create(), Guid.create()],
        },
        {
            isSelected: false,
            projectTitles: ["Project 3", "Project 2"],
            timesheetDate: new Date(),
            totalHours: 5,
            userId: Guid.parse("8d5f9c58-7738-4645-a3d9-e743a9e9f3e1"),
            status: TimesheetStatus.Submitted,
            submittedTimesheetIds: [Guid.create(), Guid.create()],
        }
    ];

    public static getUserTimesheets: IUserTimesheet[] = [
        {
            timesheetDate: moment(startOfCurrentWeek).add(1, 'day').toDate(),
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Timesheet App Template",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 8,
                            managerComments: "",
                            status: TimesheetStatus.None,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Development",
                            hours: 8,
                            managerComments: "",
                            status: TimesheetStatus.None,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        }
                    ]
                }
            ]
        },
        {
            timesheetDate: moment(startOfCurrentWeek).add(2, 'day').toDate(),
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Microsoft Teams",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 8,
                            managerComments: "",
                            status: TimesheetStatus.Submitted,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                    ]
                }
            ]
        },
        {
            timesheetDate: moment(startOfCurrentWeek).add(3, 'day').toDate(),
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Microsoft Teams",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 8,
                            managerComments: "",
                            status: TimesheetStatus.Rejected,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                    ]
                }
            ]
        },
        {
            timesheetDate: moment(startOfCurrentWeek).add(4, 'day').toDate(),
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Microsoft Teams",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 1,
                            managerComments: "",
                            status: TimesheetStatus.Approved,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                    ]
                }
            ]
        },
        {
            timesheetDate: moment(startOfCurrentWeek).add(5, 'day').toDate(),
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Microsoft Teams",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 4,
                            managerComments: "",
                            status: TimesheetStatus.Saved,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                    ]
                }
            ]
        },
    ];

    public static getResources: IResource = {
        weeklyEffortsLimit: 40,
        timesheetFreezeDayOfMonth: 10,
        dailyEffortsLimit: 8
    };
}