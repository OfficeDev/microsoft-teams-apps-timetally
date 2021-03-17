// <copyright file="authentication-metadata-api.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import axios from "./axios-decorator";
import { AxiosRequestConfig } from "axios";

/**
* Get authentication metadata from API
* @param  {String} windowLocationOriginDomain Application base URL
* @param  {String} login_hint Login hint for SSO
*/
export const getAuthenticationConsentMetadata = async (windowLocationOriginDomain: string, login_hint: string): Promise<any> => {
    let url = '/authenticationMetadata/consentUrl';
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({
        windowLocationOriginDomain: windowLocationOriginDomain,
        loginhint: login_hint
    });

    return await axios.get(url, () => void 0 ,config, false);
}