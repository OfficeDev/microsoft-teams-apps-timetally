// <copyright file="sign-in.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { RouteComponentProps } from "react-router-dom";
import { Text, Button, Flex } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import { useTranslation } from 'react-i18next';
import "./sign-in.scss";

const SignInPage: React.FunctionComponent<RouteComponentProps> = props => {
    const localize = useTranslation().t;
    const history = props.history;

    function onSignIn() {
        microsoftTeams.initialize();
        microsoftTeams.authentication.authenticate({
            url: window.location.origin + "/signin-simple-start",
            successCallback: () => {
                history.push("/");
            },
            failureCallback: (reason) => {
                history.push("/errorpage");
            }
        });
    }

    return (
        <div className="sign-in">
            <div className="sign-in-content-container">
                <Flex hAlign="center" vAlign="center">
                    <Text content={localize('signInMessage')} size="medium" />
                </Flex>
                <Flex hAlign="center" vAlign="center" className="margin-between">
                    <Button content={localize("signInText")} primary className="sign-in-button" onClick={onSignIn} />
                </Flex>
            </div>
        </div>
    );
};

export default SignInPage;

