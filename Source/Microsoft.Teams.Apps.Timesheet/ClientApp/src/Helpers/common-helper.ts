// <copyright file="common-helper.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import IUserTimesheet from "../models/fill-timesheet/user-timesheet";
import IProjectDetails from "../models/fill-timesheet/project-details";
import ITimesheetDetails from "../models/fill-timesheet/timesheet-details";

/**
* Gets the total efforts made for a calendar day.
* @param timesheet
*/
export const getTotalEfforts = (timesheet: IUserTimesheet) => {
    if (timesheet) {
        let totalHours = 0;

        if (timesheet.projectDetails) {
            timesheet.projectDetails.forEach((project: IProjectDetails) => {
                if (project.timesheetDetails) {
                    totalHours += project.timesheetDetails.reduce((timesheetHours: number, task: ITimesheetDetails) => {
                        return timesheetHours + task.hours;
                    }, 0);
                }
            });
        }

        return totalHours;
    }

    return 0;
}
