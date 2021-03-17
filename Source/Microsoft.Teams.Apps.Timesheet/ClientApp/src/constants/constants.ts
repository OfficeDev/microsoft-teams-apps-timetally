// <copyright file="constants.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

export default class Constants {
    public static readonly projectTitleMaxLength: number = 50;
    public static readonly clientNameMaxLength: number = 50;
    public static readonly taskMaxLength: number = 300;
    public static readonly taskModuleHeight: number = 746;
    public static readonly taskModuleWidth: number = 600;

    // Donut chart colors for default and dark theme.
    public static readonly billableStatusColor: string = "#61AEE5";
    public static readonly underutilizedBillableStatusColor: string = "#f58442";
    public static readonly nonBillableStatusColor: string = "#D54130";
    public static readonly underutilizedNonBillableStatusColor: string = "#858C98";

    // Donut chart colors for high contrast theme.
    public static readonly billableContrastStatusColor: string = "#1aebff";
    public static readonly underutilizedBillableContrastStatusColor: string = "#3ff23f";
    public static readonly nonBillableContrastStatusColor: string = "#ffff01";
    public static readonly underutilizedNonBillableContrastStatusColor: string = "#fff";

    // The calendar day of month on which past month timesheet will get freeze.
    public static readonly timesheetFreezeDayOfMonth: number = 10;

    // The max screen width up to which mobile view is enabled.
    public static readonly maxWidthForMobileView: number = 750;

    // The maximum efforts limit that can be filled per day.
    public static readonly dailyEffortsLimit: number = 8;

    // The maximum efforts limit that can be filled per week.
    public static readonly weeklyEffortsLimit: number = 40;

    // The maximum length manager can enter for reason's description.
    public static readonly reasonDescriptionMaxLength: number = 100;

    // Table's check-box column width.
    public static readonly tableCheckboxColumnWidth: string = "17vw";
}

// Indicates Teams theme names
export enum Themes {
    dark = "dark",
    contrast = "contrast",
    light = "light",
    default = "default"
}

// Project card navigation command.
export enum NavigationCommand {
    forward,
    backward,
    default
}

// Formating model type.
export enum ModelType {
    member,
    task
}

// Indicates UI steps rendered while creating new project.
export enum AddProjectUISteps {
    step1 = 1,
    step2 = 2
}