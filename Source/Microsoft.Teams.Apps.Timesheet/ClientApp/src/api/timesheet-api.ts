// <copyright file="timesheet-api.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { AxiosRequestConfig } from "axios";
import moment from "moment";
import axios from "../api/axios-decorator";
import IUserTimesheet from "../models/fill-timesheet/user-timesheet";

/**
 * Saves the timesheet.
 * @param timesheets The timesheet data that need to be save.
 * @param currentDate The current date.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const saveTimesheetAsync = async (timesheets: IUserTimesheet[], currentDate: Date, handleTokenAccessFailure: (error: string) => void) => {
    let requestUrl = `/api/timesheets/${moment().format("YYYY-MM-DDTHH:mm:ss")}`;
    return axios.post(requestUrl, handleTokenAccessFailure, timesheets);
}

/**
 * Submits timesheet to manager.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const submitTimesheetAsync = async (handleTokenAccessFailure: (error: string) => void) => {
    let requestUrl = `/api/timesheets/submit`;
    return axios.post(requestUrl, handleTokenAccessFailure);
}

/**
 * Duplicates efforts of source date to target dates.
 * @param sourceDate The source date of which efforts to be duplicated.
 * @param targetDates The target dates to which efforts needs to be duplicated.
 * @param currentDate The current date.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const duplicateEffortsAsync = async (sourceDate: Date, targetDates: Date[], currentDate: Date, handleTokenAccessFailure: (error: string) => void) => {
    let requestUrl = `/api/timesheets/duplicate/${moment(currentDate).format("YYYY-MM-DDTHH:mm:ss")}`;
    let data = { sourceDate, targetDates };

    return axios.post(requestUrl, handleTokenAccessFailure, data);
}

/**
 * Gets timesheets of logged-in user in specified date range.
 * @param startDate The start date from which timesheets needs to be retrieved.
 * @param endDate The end date up to which timesheets needs to be retrieved.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getTimesheetsAsync = (startDate: Date, endDate: Date, handleTokenAccessFailure: (error: string) => void) => {
    let requestUrl = "/api/timesheets";
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({ startDate, endDate });

    return axios.get(requestUrl, handleTokenAccessFailure, config);
}