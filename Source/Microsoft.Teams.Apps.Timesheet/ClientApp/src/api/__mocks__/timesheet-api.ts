// <copyright file="timesheet-api.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import IUserTimesheet from "../../models/fill-timesheet/user-timesheet";
import { StatusCodes } from "http-status-codes";
import TestData from "../../api/test-data/test-data";

/**
 * Saves the timesheet.
 * @param timesheets The timesheet data that need to be save.
 */
export const saveTimesheetAsync = async (timesheets: IUserTimesheet[], handleTokenAccessFailure: (error: string) => void) => {
    return Promise.resolve({
        data: true,
        status: StatusCodes.OK
    });
}

/**
 * Submits timesheet to manager.
 * @param timesheets The timesheet data that need to be submitted.
 */
export const submitTimesheetAsync = async (timesheets: IUserTimesheet[], handleTokenAccessFailure: (error: string) => void) => {
    return Promise.resolve({
        data: true,
        status: StatusCodes.OK
    });
}

/**
 * Duplicates efforts of source date to target dates.
 * @param sourceDate The source date of which efforts to be duplicated.
 * @param targetDates The target dates to which efforts needs to be duplicated.
 */
export const duplicateEffortsAsync = async (sourceDate: Date, targetDates: Date[], handleTokenAccessFailure: (error: string) => void) => {
    return Promise.resolve({
        data: true,
        status: StatusCodes.OK
    });
}

/**
 * Gets timesheets of logged-in user in specified date range.
 * @param startDate The start date from which timesheets needs to be retrieved.
 * @param endDate The end date up to which timesheets needs to be retrieved.
 * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
 */
export const getTimesheetsAsync = (startDate: Date, endDate: Date, handleTokenAccessFailure: (error: string) => void) => {
    return Promise.resolve({
        data: TestData.getUserTimesheets,
        status: StatusCodes.OK
    });
}