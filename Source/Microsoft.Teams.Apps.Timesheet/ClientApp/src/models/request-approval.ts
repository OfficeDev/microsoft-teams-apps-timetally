// <copyright file="project-approval.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { Guid } from "guid-typescript";

export interface IRequestApproval {
    userId: Guid;
    timesheetDate: Array<Date>,
    status: number;
    managerComments: string;
    timesheetId: Guid;
}