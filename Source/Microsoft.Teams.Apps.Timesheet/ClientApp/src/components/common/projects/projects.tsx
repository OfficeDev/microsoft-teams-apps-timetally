// <copyright file="projects.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { cloneDeep } from "lodash";
import { Text, Accordion, Flex, Input, Divider, ChevronDownMediumIcon, Dialog, Button, TrashCanIcon, AddIcon, CloseIcon, AcceptIcon } from "@fluentui/react-northstar";
import { TFunction } from "i18next";
import { WithTranslation, withTranslation, useTranslation } from "react-i18next";
import IUserTimesheet from "../../../models/fill-timesheet/user-timesheet";
import IProjectDetails from "../../../models/fill-timesheet/project-details";
import ITimesheetDetails from "../../../models/fill-timesheet/timesheet-details";
import { Guid } from "guid-typescript";
import DatePickerWrapper from "../../common/date-picker/date-picker";

import "./projects.scss";

interface IProjectsProps extends WithTranslation {
    isDisabled: boolean,
    isMobile: boolean,
    selectedCalendarDate: Date,
    timesheetData: IUserTimesheet[],
    onProjectExpandedStateChange: (projectId: string, timesheetDate: Date | undefined) => void,
    onTaskEffortChange: (timesheetDate: Date, projectId: string, taskAtIndex: number, updatedEfforts: string) => void,
    onDeleteTask: (timesheetDate: Date, projectId: string, taskAtIndex: number) => void,
    onRequestToAddNewTask: (timesheetDate: Date, projectId: string) => void,
    onNewTaskSubmit: (timesheetDate: Date, projectId: string) => void,
    onCancelCreateNewTask: (timesheetDate: Date, projectId: string) => void,
    onNewTaskNameChange: (timesheetDate: Date, event: any, projectId: string) => void,
    onNewTaskEndDateChange: (timesheetDate: Date, projectId: string, selectedEndDate: Date) => void
}

/**
 * Renders projects assigned on particular date.
 * @param props The props of type @typedef {IProjectProps}
 */
const Projects: React.FunctionComponent<IProjectsProps> = props => {
    const localize: TFunction = useTranslation().t;

    /**
     * Calculate and returns total efforts added for a project
     * @param projects The projects of which total efforts to get.
     */
    function getTotalEffortsForProject(projects: IProjectDetails[]) {
        let totalHours = 0;

        if (projects) {
            projects.forEach((project: IProjectDetails) => {
                if (project.timesheetDetails) {
                    totalHours += project.timesheetDetails.reduce((timesheetHours: number, task: ITimesheetDetails) => {
                        return timesheetHours + task.hours;
                    }, 0);
                }
            });
        }

        return localize("fillTimesheetProjectTotalHours", { hours: totalHours });
    }

    // Gets timesheet data for selected date on calendar
    function getTimesheetDataByCalendarDate() {
        let timesheetData: IUserTimesheet[] = props.timesheetData ? cloneDeep(props.timesheetData) : [];

        return timesheetData.find((timesheet: IUserTimesheet) =>
            timesheet.timesheetDate.valueOf() === props.selectedCalendarDate.valueOf());
    }

    /**
     * Renders UI for adding new task
     * @param timesheetDate The timesheet date for which task to be added.
     * @param projectDetails The project details.
     */
    function renderAddTaskUI(timesheetDate: Date, projectDetails: IProjectDetails) {
        if (projectDetails.isAddNewTaskActivated) {
            return (
                <React.Fragment>
                    {
                        props.isMobile ? (
                            <Flex className="new-task" vAlign="center">
                                <Flex column gap="gap.smaller">
                                    <Text content={localize("fillTimesheetAddNewTaskLabel")} weight="semibold" size="small" />
                                    <Input disabled={projectDetails.isAddNewTaskInProgress || props.isDisabled} placeholder={localize("fillTimesheetAddNewTaskPlaceholder")} maxLength={50} onChange={(event) => props.onNewTaskNameChange(timesheetDate, event, projectDetails.id)} />
                                </Flex>
                                <Flex column gap="gap.smaller" padding="padding.medium">
                                    <Text content={localize("fillTimesheetTaskEndDateLabel")} weight="semibold" size="small" />
                                    <DatePickerWrapper
                                        selectedDate={props.selectedCalendarDate}
                                        minDate={props.selectedCalendarDate}
                                        maxDate={projectDetails.endDate}
                                        disableSelectionForPastDate={false}
                                        theme=""
                                        onDateSelect={(selectedDate: Date) => props.onNewTaskEndDateChange(timesheetDate, projectDetails.id, selectedDate)}
                                    />
                                </Flex>
                                <Flex.Item push>
                                    <Flex gap="gap.smaller">
                                        <Button iconOnly text primary loading={projectDetails.isAddNewTaskInProgress} disabled={projectDetails.isAddNewTaskInProgress} icon={<AcceptIcon />} onClick={() => props.onNewTaskSubmit(timesheetDate, projectDetails.id)} />
                                        <Button iconOnly text disabled={projectDetails.isAddNewTaskInProgress || props.isDisabled} icon={<CloseIcon />} onClick={() => props.onCancelCreateNewTask(timesheetDate, projectDetails.id)} />
                                    </Flex>
                                </Flex.Item>
                            </Flex>
                        ) : (
                                <React.Fragment>
                                    <Flex className="new-task" vAlign="center" gap="gap.medium">
                                        <Flex column gap="gap.smaller">
                                            <Text content={localize("fillTimesheetAddNewTaskLabel")} weight="semibold" size="small" />
                                            <Input inverted disabled={projectDetails.isAddNewTaskInProgress || props.isDisabled} placeholder={localize("fillTimesheetAddNewTaskPlaceholder")} maxLength={50} design={{ minWidth: "17.75rem" }} input={{ design: { minWidth: "17.75rem" } }} onChange={(event) => props.onNewTaskNameChange(timesheetDate, event, projectDetails.id)} />
                                        </Flex>
                                        <Flex column gap="gap.smaller">
                                            <Text content={localize("fillTimesheetTaskEndDateLabel")} weight="semibold" size="small" />
                                            <DatePickerWrapper
                                                selectedDate={props.selectedCalendarDate}
                                                minDate={props.selectedCalendarDate}
                                                maxDate={projectDetails.endDate}
                                                disableSelectionForPastDate={false}
                                                theme=""
                                                onDateSelect={(selectedDate: Date) => props.onNewTaskEndDateChange(timesheetDate, projectDetails.id, selectedDate)}
                                            />
                                        </Flex>
                                        <Flex.Item push>
                                            <Flex gap="gap.smaller">
                                                <Button primary loading={projectDetails.isAddNewTaskInProgress} disabled={projectDetails.isAddNewTaskInProgress || props.isDisabled} content={localize("fillTimesheetAddNewTaskButtonText")} size="smallest" design={{ minWidth: "5.6rem" }} onClick={() => props.onNewTaskSubmit(timesheetDate, projectDetails.id)} />
                                                <Button disabled={projectDetails.isAddNewTaskInProgress || props.isDisabled} content={localize("fillTimesheetCancelNewTaskButtonText")} size="smallest" design={{ minWidth: "5.6rem" }} onClick={() => props.onCancelCreateNewTask(timesheetDate, projectDetails.id)} />
                                            </Flex>
                                        </Flex.Item>
                                    </Flex>
                                    <Divider className="tasks-separator" />
                                </React.Fragment>
                            )
                    }
                </React.Fragment>
            );
        }

        return props.isMobile ? (
            <Flex vAlign="center"><Button disabled={projectDetails.isAddNewTaskInProgress || props.isDisabled} text primary icon={<AddIcon />} design={{ margin: "0", padding: "0", minWidth: "8.6rem" }} content={localize("fillTimesheetAddNewTaskLabel")} onClick={() => props.onRequestToAddNewTask(timesheetDate, projectDetails.id)} /></Flex>)
            : null;
    }

    // Renders projects in desktop view
    function renderProjectsInDesktopView() {
        let timesheetData = getTimesheetDataByCalendarDate();

        if (!timesheetData || !timesheetData.projectDetails) {
            return [];
        }

        return timesheetData.projectDetails.map((project: IProjectDetails, index: number) => {
            return ({
                key: `project-${index}`,
                title: ({
                    content:
                        <React.Fragment>
                            {index > 0 ? <Divider className="projects-separator" size={1} /> : null}
                            <Flex vAlign="center">
                                <Flex vAlign="center" gap="gap.smaller" fill onClick={() => props.onProjectExpandedStateChange(project.id, timesheetData?.timesheetDate)}>
                                    <Text content={`${project.title} (${getTotalEffortsForProject([project])})`} weight="semibold" />
                                    {
                                        project.isProjectViewExpanded ? <ChevronDownMediumIcon rotate={180} /> : <ChevronDownMediumIcon />
                                    }
                                </Flex>
                                {
                                    project.isAddNewTaskActivated ? null : <Flex.Item push>
                                        <Button disabled={props.isDisabled} text primary icon={<AddIcon />} design={{ margin: "0", padding: "0", minWidth: "8.6rem" }} content={localize("fillTimesheetAddNewTaskLabel")} onClick={() => props.onRequestToAddNewTask(timesheetData!.timesheetDate, project.id)} />
                                    </Flex.Item>
                                }
                            </Flex>
                        </React.Fragment>,
                    className: "project",
                    indicator: { className: "accordion-indicator-disabled" }
                }),
                content: ({ content: renderProjectTasksInDesktopView(timesheetData?.timesheetDate!, project), className: "tasks-container", active: project.isProjectViewExpanded })
            });
        });
    }

    /**
     * Renders projects tasks in desktop view
     * @param timesheetDate The date of which tasks to render.
     * @param project The project of which tasks to render.
     */
    function renderProjectTasksInDesktopView(timesheetDate: Date, project: IProjectDetails) {
        if (project && project.timesheetDetails) {
            let tasks = project.timesheetDetails.map((task: ITimesheetDetails, index: number) => {
                if (task.taskId === Guid.EMPTY) {
                    return;
                }

                return (
                    <React.Fragment key={`task-${index}`}>
                        <Flex className="task" vAlign="center">
                            <Flex.Item size="size.half">
                                <Flex vAlign="center" gap="gap.small">
                                    <Flex.Item size="size.large" >
                                        <Text truncated content={task.taskTitle} title={task.taskTitle} design={{ maxWidth: "20rem" }} />
                                    </Flex.Item>
                                    <Flex gap="gap.small" vAlign="center">
                                        <Input disabled={props.isDisabled} inverted type="number" input={{ design: { width: "6.5rem" } }} min={0} value={task.hours} onChange={(event: any) => props.onTaskEffortChange(timesheetDate, project.id, index, event.target.value)} />
                                        <Text content={localize("fillTimesheetProjectTotalHoursLabel")} />
                                    </Flex>
                                </Flex>
                            </Flex.Item>
                            {
                                task.isAddedByMember ?
                                    <Flex.Item push>
                                        <Flex>
                                            <Dialog
                                                header={<Text content={localize("deleteTaskConfirmationMessage")} weight="semibold" />}
                                                cancelButton={localize("cancelButtonLabel")}
                                                confirmButton={localize("confirmationDeleteButtonLabel")}
                                                content={localize("fillTimesheetDeleteTaskDialogContent")}
                                                trigger={<Button text loading={task.isDeleteTaskInProgress} disabled={props.isDisabled} iconOnly icon={<TrashCanIcon />} />}
                                                onConfirm={() => props.onDeleteTask(timesheetDate, project.id, index)}
                                            />
                                        </Flex>
                                    </Flex.Item> : null
                            }
                        </Flex>
                        {index + 1 < project.timesheetDetails.length ? <Divider className="tasks-separator" /> : null}
                    </React.Fragment>
                );
            });

            return (
                <Flex className="project-tasks" column vAlign="center">
                    {renderAddTaskUI(timesheetDate, project)}
                    {tasks}
                </Flex>
            );
        }

        return (
            <Flex column vAlign="center" gap="gap.medium">
                {renderAddTaskUI(timesheetDate, project)}
            </Flex>
        );
    }

    // Renders projects in mobile view
    function renderProjectsInMobileView() {
        let timesheetData = getTimesheetDataByCalendarDate();

        if (!timesheetData || !timesheetData.projectDetails) {
            return [];
        }

        let panels = timesheetData.projectDetails.map((project: IProjectDetails, index: number) => {
            return ({
                key: `project-${index}`,
                title: ({
                    content:
                        <React.Fragment>
                            {index > 0 ? <Divider className="projects-separator" size={1} /> : null}
                            <Flex vAlign="center" space="between" gap="gap.smaller" onClick={() => props.onProjectExpandedStateChange(project.id, timesheetData?.timesheetDate)}>
                                <Flex column>
                                    <Text content={project.title} weight="semibold" />
                                    <Text content={localize("fillTimesheetProjectTotalTasks", { taskCount: project.timesheetDetails ? project.timesheetDetails.length : 0 })} disabled size="small" weight="semibold" />
                                </Flex>
                                <Flex.Item push>
                                    <Text content={getTotalEffortsForProject([project])} />
                                </Flex.Item>
                                {
                                    project.isProjectViewExpanded ? <ChevronDownMediumIcon rotate={180} size="small" /> : <ChevronDownMediumIcon size="small" />
                                }
                            </Flex>
                        </React.Fragment>,
                    indicator: { className: "accordion-indicator-disabled" }
                }),
                content: ({ content: renderProjectTasksInMobileView(timesheetData?.timesheetDate!, project), active: project.isProjectViewExpanded })
            });
        });

        return panels;
    }

    /**
     * Renders project tasks in mobile view
     * @param timesheetDate The date of which tasks to render.
     * @param projectDetails The project of which tasks to render.
     */
    function renderProjectTasksInMobileView(timesheetDate: Date, projectDetails: IProjectDetails) {
        if (projectDetails && projectDetails.timesheetDetails) {
            let tasks = projectDetails.timesheetDetails.map((task: ITimesheetDetails, index: number) => {
                if (task.taskId === Guid.EMPTY) {
                    return;
                }

                return (
                    <React.Fragment key={`task-${index}`}>
                        {index > 0 ? <Divider className="tasks-separator" /> : null}
                        <Flex className="task" vAlign="center" space="between">
                            <Flex column vAlign="center" gap="gap.smaller">
                                <Text content={task.taskTitle} size="small" weight="semibold" />
                                <Input disabled={props.isDisabled} type="number" input={{ design: { width: "6.5rem" } }} min={0} value={task.hours} onChange={(event: any) => props.onTaskEffortChange(timesheetDate, projectDetails.id, index, event.target.value)} />
                            </Flex>
                            {
                                task.isAddedByMember ?
                                    <Dialog
                                        design={{ width: "28rem" }}
                                        header={<Text content={localize("deleteTaskConfirmationMessage")} weight="semibold" />}
                                        cancelButton={localize("cancelButtonLabel")}
                                        confirmButton={localize("confirmationDeleteButtonLabel")}
                                        content={localize("fillTimesheetDeleteTaskDialogContent")}
                                        trigger={<Button text loading={task.isDeleteTaskInProgress} disabled={props.isDisabled} iconOnly icon={<TrashCanIcon />} />}
                                        onConfirm={() => props.onDeleteTask(timesheetDate, projectDetails.id, index)}
                                    /> : null
                            }
                        </Flex>
                    </React.Fragment>
                );
            });

            return (
                <Flex className="project-tasks" column vAlign="center" gap="gap.medium">
                    {tasks}
                    {renderAddTaskUI(timesheetDate, projectDetails)}
                </Flex>
            );
        }

        return (
            <Flex column vAlign="center" gap="gap.medium">
                {renderAddTaskUI(timesheetDate, projectDetails)}
            </Flex>
        );
    }

    // Renders component
    return (
        <Flex className="projects-container" hAlign="center">
            <Accordion className="projects" panels={props.isMobile ? renderProjectsInMobileView() : renderProjectsInDesktopView()} />
        </Flex>
    );
}

export default withTranslation()(Projects);