// <copyright file="project-task-overview.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

export default interface IProjectTaskOverview {
    id: string;
    title: string;
    projectId: string;
    totalHours: number;
    isSelected: boolean;
    isRemoved: boolean;
    startDate: Date;
    endDate: Date;

}