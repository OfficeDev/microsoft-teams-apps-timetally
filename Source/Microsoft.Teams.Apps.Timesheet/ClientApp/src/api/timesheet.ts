// <copyright file="timesheet.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import axios from "./axios-decorator";
import { IRequestApproval } from "../models/request-approval";
import { Guid } from "guid-typescript";

/**
 * Approve timesheets.
 * @param requestApproval The request approval of which timesheet status to approve.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const approveTimesheetsAsync = async (
    requestApproval: IRequestApproval[],
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/timesheets/approve`;

    return axios.post(url, handleTokenAccessFailure, requestApproval);
}

/**
 * Reject timesheets.
 * @param requestApproval The request approval of which timesheet status to reject.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const rejectTimesheetsAsync = async (
    requestApproval: IRequestApproval[],
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/timesheets/reject`;

    return axios.post(url, handleTokenAccessFailure, requestApproval);
}

/**
 * Get dashboard requests.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getDashboardRequestsAsync = async (
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/timesheets/dashboard`;
    return await axios.get(url, handleTokenAccessFailure, undefined, undefined);
};

/**
 * Gets user timesheet requests.
 * @param reporteeId The reportee Id of which requests to get.
 */
export const getUserTimesheetsAsync = async (
    reporteeId: string,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `api/timesheets/${reporteeId}/submitted`;
    return await axios.get(url, handleTokenAccessFailure);
}

/**
 * Approve timesheet requests by user Ids.
 * @param userIds The user Ids of which timesheet status to approve.
 */
export const approveTimesheetsByUserIdsAsync = async (
    userIds: Array<Guid>,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `api/timesheets/approvebyuserids`;

    return axios.patch(url, handleTokenAccessFailure, userIds);
}

/**
 * Reject timesheet requests by user Ids.
 * @param userIds The user Ids of which timesheet status to reject.
 */
export const rejectTimesheetsByUserIdsAsync = async (
    userIds: Array<Guid>,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `api/timesheets/rejectbyuserids`;

    return axios.patch(url, handleTokenAccessFailure, userIds);
}
