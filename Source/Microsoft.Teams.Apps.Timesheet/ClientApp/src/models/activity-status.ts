// <copyright file="activity-status.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

// Contains the values which shows status for an activity
export enum ActivityStatus {
    // Indicates no activity
    None,

    // Indicates that the activity completed successfully
    Success,

    // Indicates that the activity failed to execute
    Error
}