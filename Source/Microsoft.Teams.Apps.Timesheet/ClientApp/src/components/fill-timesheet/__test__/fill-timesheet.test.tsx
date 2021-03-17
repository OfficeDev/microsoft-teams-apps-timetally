// <copyright file="fill-timesheet.test.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import FillTimesheet from "../fill-timesheet";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";
import moment from "moment";

jest.mock("../../../api/project.ts");
jest.mock("../../../api/resource-api.ts");
jest.mock("../../../api/timesheet-api.ts");

jest.mock("react-i18next", () => ({
    useTranslation: () => ({
        t: (key: any) => key,
        i18n: { changeLanguage: jest.fn() }
    }),

    withTranslation: () => (Component: any) => {
        Component.defaultProps = {
            ...Component.defaultProps,
            t: (key: any) => key,
        };
        return Component;
    },
}));

jest.mock("react-router", () => ({
    withRouter: (Component: any) => {
        Component.defaultProps = {
            ...Component.defaultProps,
            t: (key: any) => key,
        };
        return Component;
    },
}));

let container: any = null;

beforeEach(() => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);

    act(() => {
        render(
            <Provider>
                <FillTimesheet />
            </Provider>,
            container
        );
    });
});

afterEach(() => {
    // cleanup on exiting
    unmountComponentAtNode(container);
    container.remove();
    container = null;
});

const startOfCurrentWeek: Date = moment().startOf('week').startOf('day').toDate();

describe("FillTimesheet", () => {
    it("renders snapshots", () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it("Tests whether duplicate efforts functionality is active", () => {
        const duplicateEffortsButton = document.querySelector("[data-testid=duplicate-efforts-button]");

        act(() => {
            duplicateEffortsButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const duplicateEffortsPlaceholder = document.querySelector("[data-testid=duplicate-efforts-placeholder]");
        expect(duplicateEffortsPlaceholder).not.toBe(null);
    });

    it("Tests whether timesheet date is selected on calendar", () => {
        const calendarDateWithApprovedEfforts: string = moment(startOfCurrentWeek).add(4, 'day').format("YYYY-MM-DD");
        const calendarDate = document.querySelector(`[data-testid=calendar-date-${calendarDateWithApprovedEfforts}]`);

        act(() => {
            calendarDate?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const selectedClassName = document.querySelector(`[data-testid=calendar-date-${calendarDateWithApprovedEfforts}]`)?.getAttribute("class");
        expect(selectedClassName).toContain("selected-date");
    });
});