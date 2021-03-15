// <copyright file="project-details.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import ITimesheetDetails from "./timesheet-details";

export default interface IProjectDetails {
    id: string,
    title: string,
    timesheetDetails: ITimesheetDetails[],
    isProjectViewExpanded?: boolean,
    isAddNewTaskActivated: boolean,
    isAddNewTaskInProgress: boolean,
    startDate: Date,
    endDate: Date
}