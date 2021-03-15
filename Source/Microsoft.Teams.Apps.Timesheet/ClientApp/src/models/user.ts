// <copyright file="user.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

export default interface IUser {
    // User's display name.
    displayName: string;

    // User's email address.
    email: string;

    // User AAD object identifier.
    id: string;
}