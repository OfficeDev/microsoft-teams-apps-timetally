// <copyright file="project-utilization.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

export default interface IProjectUtilization {
    id: string;
    title: string;
    billableUtilizedHours: number;
    billableUnderutilizedHours: number;
    nonBillableUtilizedHours: number;
    nonBillableUnderutilizedHours: number;
    totalHours: number;
    projectStartDate: Date;
    projectEndDate: Date;
}