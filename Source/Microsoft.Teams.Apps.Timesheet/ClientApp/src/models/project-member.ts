// <copyright file="project-member.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

export default interface IProjectMember {
    // Unique project Id
    projectId: string;

    // User AAD object identifier.
    userId: string;

    // Boolean indicating whether member is billable.
    isBillable: boolean;
}