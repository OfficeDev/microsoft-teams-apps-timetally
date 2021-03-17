// <copyright file="axios-decorator.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import axios, { AxiosResponse, AxiosRequestConfig } from "axios";
import baseAxios from "axios";
import * as microsoftTeams from "@microsoft/teams-js";

//Average network timeout in milliseconds.
axios.defaults.timeout = 10000;

//Application base URI.
axios.defaults.baseURL = window.location.origin;

export class AxiosJWTDecorator {
	/**
	* Post data to API
	* @param  {String} url Resource URI
    * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
	* @param  {Object} data Request body data
	*/
    public async post<T = any, R = AxiosResponse<T>>(
        url: string,
        handleTokenAccessFailure: (error: string) => void,
        data?: any,
        config?: AxiosRequestConfig
    ): Promise<R> {
        try {
            config = await this.setupAuthorizationHeader(handleTokenAccessFailure, config);
            return await axios.post(url, data, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

	/**
	* Post data to API
	* @param  {String} url Resource URI
    * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
	*/
    public async delete<T = any, R = AxiosResponse<T>>(
        url: string,
        handleTokenAccessFailure: (error: string) => void,
        config?: AxiosRequestConfig
    ): Promise<R> {
        try {
            config = await this.setupAuthorizationHeader(handleTokenAccessFailure, config);
            return await axios.delete(url, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

	/**
	* Post data to API
	* @param  {String} url Resource URI
    * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
	* @param  {Object} data Request body data
	*/
    public async put<T = any, R = AxiosResponse<T>>(
        url: string,
        handleTokenAccessFailure: (error: string) => void,
        data?: any,
        config?: AxiosRequestConfig
    ): Promise<R> {
        try {
            config = await this.setupAuthorizationHeader(handleTokenAccessFailure, config);
            return await axios.put(url, data, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

    /**
    * Post data to API
    * @param  {String} url Resource URI
    * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
    * @param  {Object} data Request body data
    */
    public async patch<T = any, R = AxiosResponse<T>>(
        url: string,
        handleTokenAccessFailure: (error: string) => void,
        data?: any,
        config?: AxiosRequestConfig
    ): Promise<R> {
        try {
            config = await this.setupAuthorizationHeader(handleTokenAccessFailure, config);
            return await axios.patch(url, data, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

	/**
	* Get data to API
	* @param  {String} url Resource URI
    * @param  {VoidFunction} handleTokenAccessFailure Call back to handle token access failure and redirect to sign-in page.
    */
    public async get<T = any, R = AxiosResponse<T>>(
        url: string,
        handleTokenAccessFailure: (error: string) => void,
        config?: AxiosRequestConfig,
        needAuthorizationHeader: boolean = true
    ): Promise<R> {
        try {
            if (needAuthorizationHeader) {
                config = await this.setupAuthorizationHeader(handleTokenAccessFailure, config);
            }
            return await axios.get(url, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

    public getAPIRequestConfigParams = (params: any) => {
        let config: AxiosRequestConfig = baseAxios.defaults;
        config.params = params;

        return config;
    }

    /**
	* Handle error occurred during API call.
	* @param  {Object} error Error response object
	*/
    private handleError(error: any): any {
        if (error.hasOwnProperty("response")) {
            return error.response;
        } else {
            return error;
        }
    }

    private async setupAuthorizationHeader(
        handleTokenAccessFailure: (error: string) => void,
        config?: AxiosRequestConfig
    ): Promise<AxiosRequestConfig> {
        microsoftTeams.initialize();

        return new Promise<AxiosRequestConfig>((resolve, reject) => {
            const authTokenRequest = {
                successCallback: (token: string) => {
                    if (!config) {
                        config = axios.defaults;
                    }
                    config.headers["Authorization"] = `Bearer ${token}`;
                    resolve(config);
                },
                failureCallback: handleTokenAccessFailure,
                resources: []
            };
            microsoftTeams.authentication.getAuthToken(authTokenRequest);
        });
    }
}

const axiosJWTDecoratorInstance = new AxiosJWTDecorator();
export default axiosJWTDecoratorInstance;