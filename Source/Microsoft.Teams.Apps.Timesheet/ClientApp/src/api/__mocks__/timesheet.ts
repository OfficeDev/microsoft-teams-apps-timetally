// <copyright file="timesheet.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { StatusCodes } from "http-status-codes";
import TestData from "../../api/test-data/test-data";
import { IRequestApproval } from "../../models/request-approval";

/**
 * Approve timesheets.
 * @param requestApproval The request approval of which timesheet status to approve.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const approveTimesheetsAsync = async () => {
    return Promise.resolve({
        status: StatusCodes.NO_CONTENT
    });
};

/**
 * Reject timesheets.
 * @param requestApproval The request approval of which timesheet status to reject.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const rejectTimesheetsAsync = async () => {
    return Promise.resolve({
        status: StatusCodes.NO_CONTENT
    });
};

/**
 * Get dashboard requests.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getDashboardRequestsAsync = async () => {
    return Promise.resolve({
        data: TestData.getDashboardRequests,
        status: StatusCodes.OK
    });
};

/**
 * Gets user timesheet requests.
 * @param reporteeId The reportee Id of which requests to get.
 */
export const getUserTimesheetsAsync = async () => {
    return Promise.resolve({
        data: TestData.getSubmittedRequests,
        status: StatusCodes.OK
    });
};