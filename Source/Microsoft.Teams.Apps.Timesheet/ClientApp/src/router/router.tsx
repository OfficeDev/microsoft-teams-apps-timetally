// <copyright file="router.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { BrowserRouter, Route, Switch } from "react-router-dom";
import ManagerDashboard from "../components/manager-dashboard/manager-dashboard";
import ManageProject from "../components/manage-project/manage-project";
import RequestReview from "../components/request-review/request-review";
import Calendar from "../components/common/calendar/calendar";
import SignInPage from "../components/signin/sign-in";
import SignInSimpleStart from "../components/signin/sign-in-start";
import SignInSimpleEnd from "../components/signin/sign-in-end";
import AddProjectPage from "../components/add-project/add-project-page";
import FillTimesheet from "../components/fill-timesheet/fill-timesheet";
import "../i18n";
import ErrorPage from "../components/error-page";

export const AppRoute: React.FunctionComponent<{}> = () => {
    return (
        <React.Suspense fallback={<div className="container-div"><div className="container-subdiv"></div></div>}>
            <BrowserRouter>
                <Switch>
                    <Route exact path="/add-project" component={AddProjectPage} />
                    <Route exact path="/manager-dashboard" component={ManagerDashboard}/>
                    <Route exact path="/request-review/:userId/:userName/:isMobileView" component={RequestReview} />
                    <Route exact path="/errorpage" component={ErrorPage} />
                    <Route exact path="/" component={Calendar} />
                    <Route exact path="/manage-project/:projectId/:isMobileView" component={ManageProject} />
                    <Route exact path="/signin" component={SignInPage} />
                    <Route exact path="/signin-simple-start" component={SignInSimpleStart} />
                    <Route exact path="/signin-simple-end" component={SignInSimpleEnd} />
                    <Route exact path="/fill-timesheet" component={FillTimesheet} />
                </Switch>
            </BrowserRouter>
        </React.Suspense>
    );
};