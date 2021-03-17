// <copyright file="manager-dashboard.test.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import ManagerDashboard from "../manager-dashboard";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";

jest.mock("../../../api/project");
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
        <ManagerDashboard />
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

describe("ManagerDashboard", () => {
  it("Renders snapshots", async () => {
    expect(pretty(container.innerHTML)).toMatchSnapshot();
  });

  it("Render requests count", async () => {
    const requestTable = document.querySelector("[data-tid=dashboard-table]");
    expect(requestTable?.childElementCount).toBe(2);
  });

  it("Approve requests", async () => {
    const firstRequestCheckbox = document.querySelector(
      "[data-tid=member-checkbox-0]"
    );

    await act(async () => {
      firstRequestCheckbox?.dispatchEvent(
        new MouseEvent("click", { bubbles: true })
      );
    });

    const approveButton = document.querySelector("[data-tid=approve-button]");

    await act(async () => {
      approveButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    const approveDialog = document.querySelector(
      "[data-tid=dialog-approve-button]"
    );
    await act(async () => {
      approveDialog?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });
    const requestTable = document.querySelector("[data-tid=dashboard-table]");
    expect(requestTable).toBeNull();
  });

  it("Reject requests", async () => {
    const firstRequestCheckbox = document.querySelector(
      "[data-tid=member-checkbox-0]"
    );
    await act(async () => {
      firstRequestCheckbox?.dispatchEvent(
        new MouseEvent("click", { bubbles: true })
      );
    });

    const approveButton = document.querySelector("[data-tid=reject-button]");
    await act(async () => {
      approveButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    const rejectDialog = document.querySelector("[data-tid=dialog-reject-button]");
    await act(async () => {
      rejectDialog?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    const requestTable = document.querySelector("[data-tid=dashboard-table]");
    expect(requestTable).toBeNull();
  });

  it("Select all requests", async () => {
    const selectAllCheckbox = document.querySelector(
      "[data-tid=select-all-requests]"
    );
    await act(async () => {
      selectAllCheckbox?.dispatchEvent(
        new MouseEvent("click", { bubbles: true })
      );
    });
    let secondRequestCheckboxToBeTrue = document.querySelector(
      "[data-tid=member-checkbox-0]"
    );
    expect(secondRequestCheckboxToBeTrue?.getAttribute("aria-checked")).toBe(
      "true"
    );

    await act(async () => {
      selectAllCheckbox?.dispatchEvent(
        new MouseEvent("click", { bubbles: true })
      );
    });
    let secondRequestCheckboxToBeFalse = document.querySelector(
      "[data-tid=member-checkbox-0]"
    );

    expect(secondRequestCheckboxToBeFalse?.getAttribute("aria-checked")).toBe(
      "false"
    );
  });

  it("Search requests", async () => {
    const searchInput = document.querySelector("[data-testid=search-input]");
    act(() => {
      var event = new KeyboardEvent("keypress", { key: "84" });
      searchInput?.firstElementChild?.firstChild?.dispatchEvent(event);
    });
    const requestTable = document.querySelector("[data-tid=dashboard-table]");
    expect(requestTable?.childElementCount).toBe(2);
  });
});
