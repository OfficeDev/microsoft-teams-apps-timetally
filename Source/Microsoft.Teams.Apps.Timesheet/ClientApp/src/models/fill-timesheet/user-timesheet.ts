// <copyright file="user-timesheet.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import IProjectDetails from "./project-details";

export default interface IUserTimesheet {
    timesheetDate: Date,
    projectDetails: IProjectDetails[]
}