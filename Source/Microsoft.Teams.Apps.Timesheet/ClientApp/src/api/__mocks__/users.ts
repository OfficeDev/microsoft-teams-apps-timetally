// <copyright file="users.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { StatusCodes } from "http-status-codes";
import TestData from "../../api/test-data/test-data";

/**
 * Get user timesheets by date range.
 * @param calendarStartDate Start date range.
 * @param calendarEndDate End date range.
 * @param userGuid User id of which project details to fetch.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getUserTimesheetsOverviewAsync = async () => {
    return Promise.resolve({
        data: TestData.getUserTimesheets,
        status: StatusCodes.NO_CONTENT
    });
};

/**
 * Search reportees
 * @param searchString Search text.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getReporteesAsync = async () => {
    return Promise.resolve({
        data: TestData.getReportees,
        status: StatusCodes.OK
    });
};

/**
 * Gets the user profiles
 * @param userIds The user IDs of which profiles to get
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getUserProfilesAsync = async () => {
    return Promise.resolve({
        data: TestData.getSubmittedRequests,
        status: StatusCodes.OK
    });
};

/**
 * Gets user timesheet.
 * @param reporteeId The reportee Id of which timesheets to get.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getUserTimesheetsAsync = async () => {
    return Promise.resolve({
        data: TestData.getSubmittedRequests,
        status: StatusCodes.OK
    });
};