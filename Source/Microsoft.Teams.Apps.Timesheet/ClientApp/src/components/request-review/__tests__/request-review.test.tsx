// <copyright file="request-review.test.ts" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import RequestReview from "../request-review";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";

jest.mock("../../../api/timesheet");
jest.mock("../../../api/users");

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

jest.mock("react-router-dom", () => ({
    withRouter: (Component: any) => {
        Component.defaultProps = {
            ...Component.defaultProps,
            t: (key: any) => key,
            match: {
                params: {
                    projectId: "8d5f9c58-7738-4645-a3d9-e743a9e9f3e1",
                    isMobileView: false,
                }
            }
        };
        return Component;
    },
}));

jest.mock("@microsoft/teams-js", () => ({
    initialize: () => {
        return true;
    },
    getContext: (callback: any) =>
        callback(
            Promise.resolve({ teamId: "ewe", entityId: "sdsd", locale: "en-US" })
        ),
}));

let container: any = null;
beforeEach(async () => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);

    await act(async () => {
        render(
            <Provider>
                <RequestReview />
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

describe("RequestReview", () => {
    it("Renders snapshots", async () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it("Render user requests count", async () => {
        const requestTable = document.querySelector(
            "[data-tid=request-review-table]"
        );
        expect(requestTable?.childElementCount).toBe(4);
    });

    it("Toggle user timesheets", async () => {
        const menu = document.querySelector(
            "[data-tid=view-timesheet-menu]"
        );
        const timesheetToggle = menu?.getElementsByTagName("a")?.item(1);
        const calendarComponentNull = document.querySelector(
            "[data-tid=calendar-component]"
        );
        await act(async () => {
            timesheetToggle?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });
        const calendarComponentNotNull = document.querySelector(
            "[data-tid=calendar-component]"
        );
        expect(calendarComponentNull).toBe(null);
        expect(calendarComponentNotNull).not.toBe(null);
    });
});
