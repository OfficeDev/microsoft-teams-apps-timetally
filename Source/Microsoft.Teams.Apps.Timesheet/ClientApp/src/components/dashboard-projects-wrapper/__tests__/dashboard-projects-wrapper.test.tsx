// <copyright file="dashboard-projects-wrapper.test.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import DashboardProjectsWrapper from "../dashboard-projects-wrapper";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";
import { IDashboardProject } from "../../../models/dashboard/dashboard-project";
import { Guid } from "guid-typescript";

jest.mock("react-i18next", () => ({
    useTranslation: () => ({
        t: (key: any) => key,
        i18n: { changeLanguage: jest.fn() },
    }),

    withTranslation: () => (Component: any) => {
        Component.defaultProps = {
            ...Component.defaultProps,
            t: (key: any) => key,
        };
        return Component;
    },
}));

let container: any = null;
let projects: IDashboardProject[] = [
    { id: Guid.create(), title: "Project 1", totalHours: 22, utilizedHours: 10 },
    { id: Guid.create(), title: "Project 2", totalHours: 100, utilizedHours: 0 },
];
beforeEach(() => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);
});

afterEach(() => {
    // cleanup on exiting
    unmountComponentAtNode(container);
    container.remove();
    container = null;
});

describe("DashboardProjectsWrapper", () => {
    it("renders snapshots", () => {
        let projects: IDashboardProject[] = [
            { id: Guid.create(), title: "Project 1", totalHours: 22, utilizedHours: 10 },
            { id: Guid.create(), title: "Project 2", totalHours: 100, utilizedHours: 0 },
        ];

        act(() => {
            render(
                <Provider>
                    <DashboardProjectsWrapper isMobileView={false} projects={projects} onProjectCardClick={() => { }} searchText="" />
                </Provider>,
                container
            );
        });

        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });
});