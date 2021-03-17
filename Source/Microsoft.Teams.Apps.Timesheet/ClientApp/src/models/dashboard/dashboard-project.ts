// <copyright file="dashboard-project.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { Guid } from "guid-typescript";

export interface IDashboardProject {
    id: Guid;
    title: string;
    utilizedHours: number;
    totalHours: number;
}