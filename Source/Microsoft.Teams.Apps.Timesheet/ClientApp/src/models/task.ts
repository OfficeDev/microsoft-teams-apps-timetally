// <copyright file="task.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

export default interface ITask {
    // Unique Id of task.
    id: string;

    // Title of the task.
    title: string;

    // Project for which task is created.
    projectId: string;

    // The efforts filled for task.
    hours?: number;

    // Start date of the task.
    startDate: Date;

    // End date of the task.
    endDate: Date;
}