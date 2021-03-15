// <copyright file="submitted-request.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { Guid } from "guid-typescript";

export interface ISubmittedRequest {
    userId: Guid;
    timesheetDate: Date;
    totalHours: number;
    isSelected: boolean;
    status?: number;
    projectTitles: string[];
    submittedTimesheetIds: Guid[];
}