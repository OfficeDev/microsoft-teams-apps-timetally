// <copyright file="dashboard-request.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { Guid } from "guid-typescript";

export interface IDashboardRequest {
    userId: Guid;
    userName: string;
    numberOfDays: number;
    totalHours: number;
    isSelected: boolean;
    status?: number;
    requestedForDates: Date[][];
    submittedTimesheetIds: Guid[];
}