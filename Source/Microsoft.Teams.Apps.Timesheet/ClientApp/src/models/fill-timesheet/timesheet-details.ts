// <copyright file="timesheet-details.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { TimesheetStatus } from "../timesheet-status";

export default interface ITimesheetDetails {
    taskId: string,
    taskTitle: string,
    hours: number,
    status: TimesheetStatus,
    managerComments: string,
    isAddedByMember: boolean,
    isDeleteTaskInProgress: boolean,
    startDate: Date,
    endDate: Date
}