// <copyright file="project.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { StatusCodes } from "http-status-codes";
import IProjectMemberOverview from "../../models/project-member-overview";
import IProjectTaskOverview from "../../models/project-task-overview";
import IProjectUtilization from "../../models/project-utilization";
import { Guid } from "guid-typescript";
import { IDashboardProject } from "../../models/dashboard/dashboard-project";

const projectUtilization: IProjectUtilization = {
    billableUtilizedHours: 30,
    nonBillableUtilizedHours: 30,
    billableUnderutilizedHours: 20,
    nonBillableUnderutilizedHours: 20,
    totalHours: 100,
    id: "1212",
    title: "Test",
    projectEndDate: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 1),
    projectStartDate: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 2)
};

const memberOverview: IProjectMemberOverview[] = [
    { id: "34344", isBillable: true, isRemoved: false, isSelected: false, projectId: "1212", totalHours: 50, userId: "1212", userName: "demo 1" },
    { id: "34345", isBillable: true, isRemoved: false, isSelected: false, projectId: "1212", totalHours: 50, userId: "1213", userName: "demo 1" }
];

const projectTaskOverview: IProjectTaskOverview[] = [
    {
        id: Guid.createEmpty().toString(), isRemoved: false, isSelected: false, projectId: "1212", title: "test", totalHours: 44, startDate: new Date(), endDate: new Date()
    }
];

const dashboardProjects: IDashboardProject[] = [
    {
        id: Guid.createEmpty(), title: "Project X", totalHours: 5, utilizedHours: 10
    }
];

/**
 * Get approved and active project details for dashboard between date range.
 */
export const getDashboardProjectsAsync = async () => {
    return Promise.resolve({
        data: dashboardProjects,
        status: StatusCodes.OK
    });
};

/**
 * Get project utilization details between date range
 */
export const getProjectUtilizationAsync = async () => {
    return Promise.resolve({
        data: projectUtilization,
        status: StatusCodes.OK
    });
};

/**
 * The API which handles request to add members.
 */
export const addMembersAsync = async () => {
    return Promise.resolve({
        data: true,
        status: StatusCodes.OK
    });
};

/**
 * The API which handles request to update members.
 */
export const deleteMembersAsync = async () => {
    return Promise.resolve({
        data: true,
        status: StatusCodes.NO_CONTENT
    });
};

/**
 * The API which handles request to update task.
 */
export const deleteTasksAsync = async () => {
    return Promise.resolve({
        data: true,
        status: StatusCodes.NO_CONTENT
    });
};

/**
 * Get approved and active project members overview between date range.
 */
export const getProjectMembersOverviewAsync = async () => {
    return Promise.resolve({
        data: memberOverview,
        status: StatusCodes.OK
    });
};

/**
 * The API which handles request to create new tasks.
 * @param tasks The details of tasks to be created.
 */
export const createTasksAsync = async (
    projectId: string,
    tasks: IProjectTaskOverview[],
    handleTokenAccessFailure: (error: string) => void) => {
    tasks.map((task: IProjectTaskOverview) => {
        task.id = Guid.create().toString();
        projectTaskOverview.push(task);
    });
    return Promise.resolve({
        data: true,
        status: StatusCodes.CREATED
    });
};

/**
 * The API which handles request to update task.
 */
export const updateTasksAsync = async () => {
    return Promise.resolve({
        data: true,
        status: StatusCodes.OK
    });
};

/**
 * Get approved and active project tasks overview between date range.
 */
export const getProjectTasksOverviewAsync = async () => {
    return Promise.resolve({
        data: projectTaskOverview,
        status: StatusCodes.OK
    });
};

/**
 * Adds a new task for a project.
 */
export const addMemberTaskAsync = async () => {
    return Promise.resolve({
        data: true,
        status: StatusCodes.OK
    });
}

/**
 * Deletes a task created by project member.
 */
export const deleteMemberTaskAsync = async () => {
    return Promise.resolve({
        data: true,
        status: StatusCodes.OK
    });
}