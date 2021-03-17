// <copyright file="project-member-overview.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

export default interface IProjectMemberOverview {
    id: string;
    userName: string;
    userId: string;
    projectId: string;
    isBillable: boolean
    totalHours: number;
    isSelected: boolean;
    isRemoved: boolean;
}