// <copyright file="project.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import ITask from "./task";
import IProjectMember from "./project-member";

export default interface IProject {
    // Unique project Id.
    id: string;

    // Project title.
    title: string;

    // Name of client associated with project.
    clientName: string;

    // Billable hours per month for project.
    billableHours: number;

    // Non-billable hours per month for project.
    nonBillableHours: number;

    // Project start date regardless of time zone.
    startDate: Date;

    // Project end date regardless of time zone.
    endDate: Date;

    // Array of tasks created for the project.
    tasks: ITask[],

    // Array of project members.
    members: IProjectMember[]
}