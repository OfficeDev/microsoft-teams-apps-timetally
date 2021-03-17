// <copyright file="create-edit-project.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import axios from "./axios-decorator";
import { AxiosRequestConfig } from "axios";
import IProject from "../models/project";

/**
 * Save project details
 * @param projectDetails Project details
 */
export const saveProject = async (projectDetails: IProject, handleTokenAccessFailure: (error: string) => void) => {
    let url = '/api/projects';

    return await axios.post(url, handleTokenAccessFailure, projectDetails);
}

/**
 * Get project details by Id
 * @param projectId Project unique Id
 */
export const getProjectDetailsById = async (projectId: string, handleTokenAccessFailure: (error: string) => void) => {
    let url = '/api/projects';
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({ projectId: projectId });

    return await axios.get(url, handleTokenAccessFailure, config);
}