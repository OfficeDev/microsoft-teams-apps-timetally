// <copyright file="toast-notification.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { ActivityStatus } from "./activity-status";

export default interface IToastNotification {
    id: number
    message: string,
    type: ActivityStatus
}