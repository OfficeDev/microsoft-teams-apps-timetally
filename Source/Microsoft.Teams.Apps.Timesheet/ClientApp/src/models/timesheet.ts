// <copyright file="timesheet.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { TimesheetStatus } from "./timesheet-status"

export default interface ITimesheet {
    date: Date,
    hours: number,
    status: TimesheetStatus
}