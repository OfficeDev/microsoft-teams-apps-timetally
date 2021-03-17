// <copyright file="users.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import axios from "./axios-decorator";
import { AxiosRequestConfig } from "axios";
import { TimesheetStatus } from "../models/timesheet-status";


/**
 * Get user timesheets by date range.
 * @param calendarStartDate Start date range.
 * @param calendarEndDate End date range.
 * @param userGuid User id of which project details to fetch.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getUserTimesheetsOverviewAsync = async (
    calendarStartDate: Date,
    calendarEndDate: Date,
    userObjectId: string,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/users/${userObjectId}/timesheets`;
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({ calendarStartDate: calendarStartDate, calendarEndDate: calendarEndDate });

    return axios.get(url, handleTokenAccessFailure, config);
};

/**
 * Search reportees
 * @param searchString Search text.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getReporteesAsync = async (searchString: string, handleTokenAccessFailure: (error: string) => void) => {
    let url = '/api/users/me/reportees';
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({ searchString: searchString });

    return await axios.get(url, handleTokenAccessFailure, config);
};

/**
 * Gets the user profiles
 * @param userIds The user IDs of which profiles to get
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getUserProfilesAsync = async (userIds: Array<string>, handleTokenAccessFailure: (error: string) => void) => {
    let url = '/api/users';
    return await axios.post(url, handleTokenAccessFailure, userIds);
};

/**
 * Gets user timesheet.
 * @param reporteeId The reportee Id of which timesheets to get.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getUserTimesheetsAsync = async (
    reporteeId: string,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `api/users/${reporteeId}/timesheets/${TimesheetStatus.Submitted}`;
    return await axios.get(url, handleTokenAccessFailure);
}