// <copyright file="manage-project.test.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import ManageProject from "../manage-project";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";

jest.mock("../../../api/project");
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

jest.mock("@microsoft/teams-js", () => ({
  initialize: () => {
    return true;
  },
  getContext: (callback: any) =>
    callback(
      Promise.resolve({ teamId: "ewe", entityId: "sdsd", locale: "en-US" })
    ),
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

jest.mock("react-donut", () => {
    Donut: {
        return "DONUT";
    }
});

let container: any = null;
beforeEach(async () => {
  // setup a DOM element as a render target
  container = document.createElement("div");
  // container *must* be attached to document so events work correctly.
  document.body.appendChild(container);

  await act(async () => {
    render(
      <Provider>
        <ManageProject />
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

describe("ManageProjectWrapper", () => {
  it("renders snapshots", async () => {
    expect(pretty(container.innerHTML)).toMatchSnapshot();
  });

  it("removes member", async () => {
    const firstMember = document.querySelector("[data-tid=remove-member-0]");

    await act(async () => {
      firstMember?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    const dialogRemoveButton = document.querySelector(
      "[data-tid=confirm-member-remove]"
    );

    await act(async () => {
      dialogRemoveButton?.dispatchEvent(
        new MouseEvent("click", { bubbles: true })
      );
    });

    const memberTable = document.querySelector("[data-tid=member-table]");
    expect(memberTable?.childElementCount).toBe(2);
  });

  it("removes task", async () => {
    const firstTask = document.querySelector("[data-tid=remove-task-0]");

    await act(async () => {
      firstTask?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    const dialogRemoveButton = document.querySelector(
      "[data-tid=confirm-remove-button]"
    );

    await act(async () => {
      dialogRemoveButton?.dispatchEvent(
        new MouseEvent("click", { bubbles: true })
      );
    });

    const taskTable = document.querySelector("[data-tid=task-table]");
    expect(taskTable).toBe(null);
  });

  it("adds new task", async () => {
    const addNewTaskButton = document.querySelector("[data-tid=addTaskButton]");

    await act(async () => {
      addNewTaskButton?.dispatchEvent(
        new MouseEvent("click", { bubbles: true })
      );
    });

    const addRowButton = document.querySelector("[data-tid=addRowButton]");
    await act(async () => {
      addRowButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });
    const taskInput = document.querySelector("[data-tid=task-title-0]");

    act(() => {
      let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
        window.HTMLInputElement.prototype,
        "value"
      )?.set;
      nativeInputValueSetter?.call(
        taskInput?.firstElementChild?.firstChild,
        "Random"
      );
      let ev = new Event("input", { bubbles: true });
      taskInput?.firstElementChild?.firstChild?.dispatchEvent(ev);
    });

    const doneTask = document.querySelector("[data-tid=submitTasks]");
    await act(async () => {
      doneTask?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    const taskTable = document.querySelector("[data-tid=task-table]");
    expect(taskTable?.childElementCount).toBe(2);
  });
});
