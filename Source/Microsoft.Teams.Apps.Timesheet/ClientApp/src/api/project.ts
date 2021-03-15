// <copyright file="project.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import axios from "./axios-decorator";
import IProjectMember from "../models/project-member";
import IProjectMemberOverview from "../models/project-member-overview";
import IProjectTaskOverview from "../models/project-task-overview";
import { AxiosRequestConfig } from "axios";
import TimesheetDetails from "../models/fill-timesheet/timesheet-details";

/**
 * Get project utilization details between date range.
 * @param projectId The project Id of which project details to get.
 * @param startDate The start date of the date range.
 * @param endDate The end date of the date range.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getProjectUtilizationAsync = async (
    projectId: string,
    startDate: Date,
    endDate: Date,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/projects/${projectId}/utilization`;
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({
        startDate: startDate,
        endDate: endDate
    });

    return axios.get(url, handleTokenAccessFailure, config);
}

/**
 * The API which handles request to add members.
 * @param projectId The Id of the project in which members need to be added.
 * @param members The details of users to be added.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const addMembersAsync = async (
    projectId: string,
    members: Array<IProjectMember>,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/projects/${projectId}/members`;

    return axios.post(url, handleTokenAccessFailure, members);
};


/**
 * The API which handles request to update members.
 * @param projectId The Id of the project in which members need to be updated.
 * @param members The details of members to be updated.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const deleteMembersAsync = async (
    projectId: string,
    members: Array<IProjectMemberOverview>,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/projects/${projectId}/deletemembers`;

    return axios.post(url, handleTokenAccessFailure, members);
};

/**
 * Get approved and active project members overview between date range.
 * @param projectId The project Id of which details to fetch.
 * @param startDate The start date of the date range.
 * @param endDate The end date of the date range.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getProjectMembersOverviewAsync = async (
    projectId: string,
    startDate: Date,
    endDate: Date,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/projects/${projectId}/membersoverview`;
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({
        startDate: startDate,
        endDate: endDate
    });

    return axios.get(url, handleTokenAccessFailure, config);
};

/**
 * The API which handles request to create new tasks.
 * @param projectId The Id of the project in which tasks need to be created.
 * @param tasks The details of tasks to be created.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const createTasksAsync = async (
    projectId: string,
    tasks: Array<IProjectTaskOverview>,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/projects/${projectId}/tasks`;

    return axios.post(url, handleTokenAccessFailure, tasks);
};

/**
 * The API which handles request to update task.
 * @param projectId The Id of the project in which tasks need to be updated.
 * @param tasks The details of tasks to be updated.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const deleteTasksAsync = async (
    projectId: string,
    taskIds: Array<string>,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/projects/${projectId}/deletetasks`;

    return axios.post(url, handleTokenAccessFailure, taskIds);
};

/**
 * Get approved and active project tasks overview between date range.
 * @param projectId The project Id of which details to fetch.
 * @param startDate The start date of the date range.
 * @param endDate The end date of the date range.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getProjectTasksOverviewAsync = async (
    projectId: string,
    startDate: Date,
    endDate: Date,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/projects/${projectId}/tasksoverview`;
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({
        startDate: startDate,
        endDate: endDate
    });

    return axios.get(url, handleTokenAccessFailure, config);
};

/**
 * Adds a new task for a project.
 * @param timesheetDetails The timesheet details.
 * @param projectId The project Id.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const addMemberTaskAsync = async (timesheetDetails: TimesheetDetails, projectId: string, handleTokenAccessFailure: (error: string) => void) => {
    let requestUrl = `/api/projects/${projectId}/member/tasks`;
    return axios.post(requestUrl, handleTokenAccessFailure, timesheetDetails);
}

/**
 * Deletes a task created by project member.
 * @param projectId The project Id of which task to be deleted.
 * @param taskId The task Id to be deleted.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const deleteMemberTaskAsync = async (projectId: string, taskId: string, handleTokenAccessFailure: (error: string) => void) => {
    let requestUrl = `/api/projects/${projectId}/tasks/${taskId}`;
    return axios.delete(requestUrl, handleTokenAccessFailure);
}

/**
 * Get approved and active project details for dashboard between date range.
 * @param startDate The start date of the date range.
 * @param endDate The end date of the date range.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getDashboardProjectsAsync = async (
    startDate: Date,
    endDate: Date,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/projects/dashboard`;
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({
        startDate: startDate,
        endDate: endDate
    });

    return axios.get(url, handleTokenAccessFailure, config);
};