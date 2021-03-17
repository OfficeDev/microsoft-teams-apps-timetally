// <copyright file="fill-timesheet.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import moment from "moment";
import { cloneDeep } from "lodash";
import { TFunction } from "i18next";
import { WithTranslation, withTranslation } from "react-i18next";
import { Flex, Button, Text, Dialog } from "@fluentui/react-northstar";
import Calendar from "../../components/common/calendar/calendar";
import Projects from "../../components/common/projects/projects";
import StatusBar from "../../components/common/toast-notification/toast-notification";
import Constants from "../../constants/constants";
import IUserTimesheet from "../../models/fill-timesheet/user-timesheet";
import IProjectDetails from "../../models/fill-timesheet/project-details";
import ITimesheetDetails from "../../models/fill-timesheet/timesheet-details";
import IToastNotification from "../../models/toast-notification";
import { ActivityStatus } from "../../models/activity-status";
import ITimesheet from "../../models/timesheet";
import { TimesheetStatus } from "../../models/timesheet-status";
import { getTimesheetsAsync } from "../../api/timesheet-api";
import { saveTimesheetAsync, submitTimesheetAsync, duplicateEffortsAsync } from "../../api/timesheet-api";
import { StatusCodes } from "http-status-codes";
import { Guid } from "guid-typescript";
import { addMemberTaskAsync, deleteMemberTaskAsync } from "../../api/project";
import { getResources } from "../../api/resource-api";
import ITask from "../../models/task";
import IResource from "../../models/resource";
import IKeyValue from "../../models/key-value";
import { getTotalEfforts } from "../../Helpers/common-helper";
import { RouteComponentProps, withRouter } from "react-router";

import "./fill-timesheet.scss";

interface IFillTimesheetState {
    isMobileView: boolean,
    isCalendarInEditMode: boolean,
    timesheetDataForCalendar: IUserTimesheet[],
    timesheetDataForProjects: IUserTimesheet[],
    notification: IToastNotification,
    selectedDateInCalendar: ITimesheet,
    isSavingTimesheet: boolean,
    isSubmittingTimesheet: boolean,
    dataToSaveOrSubmit: IUserTimesheet[],
    isDuplicatingEfforts: boolean,
    renderedWeek: number,
    isAddTaskInProgress: boolean,
    isDeleteTaskInProgress: boolean,
    isTimesheetDisabled: boolean,
    selectedWeekdaysOnCalendar: ITimesheet[],
    isUserTimesheetsLoading: boolean
}

interface IFillTimesheetProps extends WithTranslation, RouteComponentProps {
}

// This class manages activities related to fill timesheet
class FillTimesheet extends React.Component<IFillTimesheetProps, IFillTimesheetState> {
    readonly localize: TFunction;
    applicationSettings: IResource;

    constructor(props: IFillTimesheetProps) {
        super(props);
        this.localize = this.props.t;
        this.applicationSettings = { timesheetFreezeDayOfMonth: Constants.timesheetFreezeDayOfMonth, weeklyEffortsLimit: Constants.weeklyEffortsLimit, dailyEffortsLimit: Constants.dailyEffortsLimit };

        this.loadApplicationSettings = this.loadApplicationSettings.bind(this);
        this.getUserTimesheetsAsync = this.getUserTimesheetsAsync.bind(this);
        this.getManagerComments = this.getManagerComments.bind(this);
        this.getTimesheetStatus = this.getTimesheetStatus.bind(this);
        this.isControlDisabled = this.isControlDisabled.bind(this);
        this.isTimesheetExceededEffortsLimit = this.isTimesheetExceededEffortsLimit.bind(this);
        this.isDuplicateEffortsExceedWeeklyLimit = this.isDuplicateEffortsExceedWeeklyLimit.bind(this);
        this.handleTokenAccessFailure = this.handleTokenAccessFailure.bind(this);
        this.areTimesheetsAvailableToSubmit = this.areTimesheetsAvailableToSubmit.bind(this);
        this.onScreenResize = this.onScreenResize.bind(this);
        this.onEffortsDuplicated = this.onEffortsDuplicated.bind(this);
        this.onCalendarEditModeChange = this.onCalendarEditModeChange.bind(this);
        this.onProjectExpandedStateChange = this.onProjectExpandedStateChange.bind(this);
        this.onTaskEffortChange = this.onTaskEffortChange.bind(this);
        this.onSubmitTimesheet = this.onSubmitTimesheet.bind(this);
        this.onCalendarActiveDateChange = this.onCalendarActiveDateChange.bind(this);
        this.onSaveTimesheet = this.onSaveTimesheet.bind(this);
        this.onWeekChange = this.onWeekChange.bind(this);
        this.onDeleteTask = this.onDeleteTask.bind(this);
        this.onRequestToAddNewTask = this.onRequestToAddNewTask.bind(this);
        this.onNewTaskNameChange = this.onNewTaskNameChange.bind(this);
        this.onNewTaskSubmit = this.onNewTaskSubmit.bind(this);
        this.onCancelCreateNewTask = this.onCancelCreateNewTask.bind(this);
        this.onNewTaskEndDateChange = this.onNewTaskEndDateChange.bind(this);
        this.renderMobileView = this.renderMobileView.bind(this);
        this.renderCalendarDateInfo = this.renderCalendarDateInfo.bind(this);
        this.renderDesktopView = this.renderDesktopView.bind(this);
        this.onSelectedDatesChange = this.onSelectedDatesChange.bind(this);

        this.state = {
            isMobileView: window.outerWidth <= Constants.maxWidthForMobileView,
            isCalendarInEditMode: false,
            timesheetDataForCalendar: [],
            timesheetDataForProjects: [],
            notification: { id: 0, message: "", type: ActivityStatus.None },
            selectedDateInCalendar: { date: moment().toDate(), hours: 0, status: TimesheetStatus.None },
            isSavingTimesheet: false,
            isSubmittingTimesheet: false,
            dataToSaveOrSubmit: [],
            isDuplicatingEfforts: false,
            renderedWeek: moment().week(),
            isAddTaskInProgress: false,
            isDeleteTaskInProgress: false,
            isTimesheetDisabled: false,
            selectedWeekdaysOnCalendar: [],
            isUserTimesheetsLoading: true
        }
    }

    // Called when component get mounted
    componentDidMount() {
        window.addEventListener("resize", this.onScreenResize);
        this.loadApplicationSettings().finally(this.getUserTimesheetsAsync);
    }

    // Called when component get unmounted
    componentWillUnmount() {
        window.removeEventListener("resize", this.onScreenResize);
    }

    // Gets the application settings.
    private async loadApplicationSettings() {
        let apiResponse = await getResources(this.handleTokenAccessFailure);

        if (apiResponse.status === StatusCodes.OK && apiResponse.data) {
            this.applicationSettings = apiResponse.data as IResource;

            let totalDaysInCurrentMonth = moment().daysInMonth();

            // If specified timesheet freeze day of month is greater than total days in current month, then reset
            // timesheet freeze day to last day of client current month.
            if (this.applicationSettings.timesheetFreezeDayOfMonth > totalDaysInCurrentMonth) {
                this.applicationSettings.timesheetFreezeDayOfMonth = totalDaysInCurrentMonth;
            }
        }
    }

    // Gets active projects and tasks in specified date range assigned to logged-in user.
    private async getUserTimesheetsAsync() {
        this.setState({ isUserTimesheetsLoading: true });

        let startDateOfWeek = moment().week(this.state.renderedWeek).startOf('week').startOf('day').toDate();
        let endDateOfWeek = moment().week(this.state.renderedWeek).endOf('week').startOf('day').toDate();

        let apiResponse = await getTimesheetsAsync(startDateOfWeek, endDateOfWeek, this.handleTokenAccessFailure);

        if (apiResponse.status === StatusCodes.OK) {
            let userTimesheets: IUserTimesheet[] = apiResponse.data;
            let dataToSaveOrSubmit: IUserTimesheet[] = this.state.dataToSaveOrSubmit
                && this.state.dataToSaveOrSubmit.length > 0 ? cloneDeep(this.state.dataToSaveOrSubmit) : [];

            if (userTimesheets) {
                let timesheetDataForCalendar: IUserTimesheet[] = this.state.timesheetDataForCalendar
                    && this.state.timesheetDataForCalendar.length > 0 ? cloneDeep(this.state.timesheetDataForCalendar) : [];

                for (let i = 0; i < userTimesheets.length; i++) {

                    userTimesheets[i].timesheetDate = moment(userTimesheets[i].timesheetDate).startOf('day').toDate();

                    userTimesheets[i]?.projectDetails?.forEach((project: IProjectDetails) => {
                        project.isProjectViewExpanded = true;
                        project.startDate = moment(project.startDate).startOf('day').toDate();
                        project.endDate = moment(project.endDate).startOf('day').toDate();

                        project.timesheetDetails?.forEach((timesheet: ITimesheetDetails) => {
                            timesheet.startDate = moment(timesheet.startDate).startOf('day').toDate();
                            timesheet.endDate = moment(timesheet.endDate).startOf('day').toDate();
                        });
                    });

                    var timesheetAtIndex = timesheetDataForCalendar.findIndex((timesheet: IUserTimesheet) =>
                        timesheet.timesheetDate.valueOf() === userTimesheets[i].timesheetDate.valueOf());

                    if (timesheetAtIndex > -1) {
                        timesheetDataForCalendar[timesheetAtIndex] = userTimesheets[i];
                    }
                    else {
                        timesheetDataForCalendar.push(userTimesheets[i]);
                    }
                }

                let timesheetDataForProjects: IUserTimesheet[] = cloneDeep(timesheetDataForCalendar);

                if (dataToSaveOrSubmit && dataToSaveOrSubmit.length > 0) {
                    for (let i = 0; i < timesheetDataForProjects.length; i++) {
                        let filledTimesheetDetails = dataToSaveOrSubmit.find((timesheetData: IUserTimesheet) =>
                            timesheetData && timesheetData.timesheetDate.valueOf() === timesheetDataForProjects[i].timesheetDate.valueOf());

                        if (filledTimesheetDetails) {
                            timesheetDataForProjects[i] = filledTimesheetDetails;
                        }
                    }
                }

                this.setState({ timesheetDataForCalendar, timesheetDataForProjects, isUserTimesheetsLoading: false });
            }
            else {
                this.setState({ isUserTimesheetsLoading: false });
            }
        }
        else {
            this.setState({ isUserTimesheetsLoading: false });
        }
    }

    /** Get manager comment on timesheet for selected calendar date. */
    private getManagerComments() {
        let timesheetDataForCalendar = this.state.timesheetDataForCalendar ? [...this.state.timesheetDataForCalendar] : [];
        let timesheetData = timesheetDataForCalendar.find((timesheet: IUserTimesheet) =>
            moment(timesheet.timesheetDate).startOf("day").valueOf() === moment(this.state.selectedDateInCalendar.date).startOf("day").valueOf());

        let managerComments = "-";

        if (timesheetData && timesheetData.projectDetails) {
            timesheetData.projectDetails.forEach(project => {
                if (project.timesheetDetails) {
                    let timesheet = project.timesheetDetails.find((x: ITimesheetDetails) => x.managerComments?.trim()?.length > 0);

                    if (timesheet) {
                        managerComments = timesheet.managerComments;
                        return;
                    }
                }
            });
        }

        return managerComments;
    }

    private getTimesheetStatus() {
        switch (this.state.selectedDateInCalendar.status) {
            case TimesheetStatus.Approved:
                return this.localize("TimesheetStatusApproved");

            case TimesheetStatus.Rejected:
                return this.localize("TimesheetStatusRejected");

            case TimesheetStatus.Saved:
                return this.localize("TimesheetStatusSaved");

            case TimesheetStatus.Submitted:
                return this.localize("TimesheetStatusSubmitted");

            default:
                return this.localize("TimesheetStatusNotFilled");
        }
    }

    // Returns boolean value indicating whether a control should be disabled.
    private isControlDisabled() {
        return this.state.isDuplicatingEfforts
            || this.state.isSavingTimesheet
            || this.state.isSubmittingTimesheet
            || this.state.isCalendarInEditMode
            || this.state.isAddTaskInProgress
            || this.state.isDeleteTaskInProgress
            || this.state.isTimesheetDisabled
    }

    // Indicates whether there are timesheets available to submit.
    private areTimesheetsAvailableToSubmit() {
        if (this.state.dataToSaveOrSubmit && this.state.dataToSaveOrSubmit.length > 0) {
            return true;
        }

        let timesheets: IUserTimesheet[] = this.state.timesheetDataForCalendar;

        for (let i = 0; i < timesheets.length; i++) {
            for (let j = 0; j < timesheets[i].projectDetails.length; j++) {
                let hasSavedTimesheet: boolean = timesheets[i].projectDetails[j].timesheetDetails.some((x: ITimesheetDetails) => x.status === TimesheetStatus.Saved);

                if (hasSavedTimesheet) {
                    return true;
                }
            }
        }

        return false;
    }

    /**
     * Copies newly created task details to other dates within the task date range or delete tasks.
     * @param taskToInsertOrDelete The task details to be inserted or deleted.
     * @param projectId The project Id into which task was created.
     * @param userTimesheets The list of timesheets.
     * @param isDeleteTask Indicated whether to delete tasks.
     */
    private copyOrDeleteTask(taskToInsertOrDelete: ITimesheetDetails, projectId: string, userTimesheets: IUserTimesheet[], isDeleteTask: boolean = false) {
        if (taskToInsertOrDelete && projectId && userTimesheets) {
            let timesheetsWithingNewTaskRange = userTimesheets.filter((userTimesheet: IUserTimesheet) =>
                userTimesheet.timesheetDate.valueOf() >= taskToInsertOrDelete.startDate.valueOf()
                && userTimesheet.timesheetDate.valueOf() <= taskToInsertOrDelete.endDate.valueOf());

            timesheetsWithingNewTaskRange.forEach((userTimesheet: IUserTimesheet) => {
                if (userTimesheet && userTimesheet.projectDetails) {
                    let projectDetails = userTimesheet.projectDetails.find((project: IProjectDetails) =>
                        project.id === projectId);

                    if (projectDetails && projectDetails.timesheetDetails) {
                        let timesheetAtIndex = projectDetails.timesheetDetails.findIndex((timesheet: ITimesheetDetails) =>
                            timesheet.taskId === taskToInsertOrDelete.taskId);

                        if (isDeleteTask) {
                            if (timesheetAtIndex > -1 && projectDetails.timesheetDetails[timesheetAtIndex].isAddedByMember) {
                                projectDetails.timesheetDetails.splice(timesheetAtIndex, 1);
                            }
                        }
                        else {
                            if (timesheetAtIndex === -1) {
                                projectDetails.timesheetDetails.push({
                                    taskId: taskToInsertOrDelete.taskId,
                                    taskTitle: taskToInsertOrDelete.taskTitle,
                                    hours: taskToInsertOrDelete.hours,
                                    status: TimesheetStatus.None,
                                    isAddedByMember: true,
                                    isDeleteTaskInProgress: false,
                                    managerComments: "",
                                    startDate: new Date(taskToInsertOrDelete.startDate),
                                    endDate: new Date(taskToInsertOrDelete.endDate)
                                });
                            }
                        }
                    }
                }
            });
        }
    }

    /**
     * Checks whether filled timesheet exceeded daily and weekly efforts limit.
     * @param userTimesheets The timesheet details.
     */
    private isTimesheetExceededEffortsLimit(userTimesheets: IUserTimesheet[]) {
        let effortsPerWeek: IKeyValue[] = [];

        for (let i = 0; i < userTimesheets?.length; i++) {
            let weekNumber = moment().week(moment(userTimesheets[i].timesheetDate).week()).format("WW");

            let weekAtIndex = effortsPerWeek.findIndex((record: IKeyValue) => record.key === weekNumber);
            let totalEffortsOfWeekday = getTotalEfforts(userTimesheets[i]);

            if (totalEffortsOfWeekday > this.applicationSettings.dailyEffortsLimit) {
                this.setState((prevState: IFillTimesheetState) => ({
                    notification: {
                        id: prevState.notification.id + 1,
                        message: this.localize("fillTimesheetDailyLimitExceededError", { date: moment(userTimesheets[i].timesheetDate).format("DD MMM, YYYY"), maxDailyEfforts: this.applicationSettings.dailyEffortsLimit }),
                        type: ActivityStatus.Error
                    }
                }));

                return true;
            }

            if (weekAtIndex > -1) {
                effortsPerWeek[weekAtIndex].value += totalEffortsOfWeekday;
            }
            else {
                effortsPerWeek.push({ key: weekNumber, value: totalEffortsOfWeekday });
            }
        }

        if (effortsPerWeek && effortsPerWeek.length > 0) {
            let exceededLimitWeekDetails = effortsPerWeek.find((weekDetails: IKeyValue) => weekDetails.value > this.applicationSettings.weeklyEffortsLimit);

            if (exceededLimitWeekDetails) {
                this.setState((prevState: IFillTimesheetState) => ({
                    notification: {
                        id: prevState.notification.id + 1,
                        message: this.localize("fillTimesheetWeeklyLimitExceededError", { weekNumber: exceededLimitWeekDetails?.key, effortsOfWeek: exceededLimitWeekDetails?.value, maxEffortsPerWeek: this.applicationSettings.weeklyEffortsLimit }),
                        type: ActivityStatus.Error
                    }
                }));

                return true;
            }
        }

        return false;
    }

    /**
     * Checks whether duplicating efforts will exceeds the weekly limit.
     * @param sourceDateEfforts The source date total efforts.
     * @param targetDates The target dates to which source efforts to be copied.
     */
    private isDuplicateEffortsExceedWeeklyLimit(sourceDateEfforts: number, targetDates: Date[]) {
        if (sourceDateEfforts === 0) {
            return false;
        }

        let effortsPerWeek: IKeyValue[] = [];
        let calendarData: IUserTimesheet[] = this.state.timesheetDataForCalendar ? this.state.timesheetDataForCalendar : [];

        for (let i = 0; i < targetDates?.length; i++) {
            let weekNumber = moment(targetDates[i]).isoWeek();

            let weekAtIndex = effortsPerWeek.findIndex((record: IKeyValue) => record.key === weekNumber);

            if (weekAtIndex > -1) {
                effortsPerWeek[weekAtIndex].value += sourceDateEfforts;
            }
            else {
                let calendarDataWithoutTargetDate = calendarData.filter((timesheet: IUserTimesheet) => timesheet.timesheetDate.valueOf() !== moment(targetDates[i]).startOf('day').toDate().valueOf()
                    && timesheet.timesheetDate.valueOf() >= moment(targetDates[i]).startOf('week').startOf('day').toDate().valueOf()
                    && timesheet.timesheetDate.valueOf() <= moment(targetDates[i]).endOf('week').startOf('day').toDate().valueOf()
                );

                let filledEfforts = calendarDataWithoutTargetDate.reduce((timesheetHours: number, timesheet: IUserTimesheet) => {
                    return timesheetHours + getTotalEfforts(timesheet);
                }, 0);

                filledEfforts += sourceDateEfforts;

                effortsPerWeek.push({ key: weekNumber, value: filledEfforts });
            }
        }

        if (effortsPerWeek && effortsPerWeek.length > 0) {
            let exceededLimitWeekDetails = effortsPerWeek.find((weekDetails: IKeyValue) => weekDetails.value > this.applicationSettings.weeklyEffortsLimit);

            if (exceededLimitWeekDetails) {
                this.setState((prevState: IFillTimesheetState) => ({
                    notification: {
                        id: prevState.notification.id + 1,
                        message: this.localize("fillTimesheetWeeklyLimitExceededError", { weekNumber: exceededLimitWeekDetails?.key, effortsOfWeek: exceededLimitWeekDetails?.value, maxEffortsPerWeek: this.applicationSettings.weeklyEffortsLimit }),
                        type: ActivityStatus.Error
                    }
                }));

                return true;
            }
        }

        return false;
    }

    private handleTokenAccessFailure(error: string) {
        this.props.history.push("/signin");
    }

    // Returns the comma separated date string of dates which are selected on calendar while duplicating efforts.
    private getSelectedDatesToDuplicateEfforts() {
        if (this.state.selectedWeekdaysOnCalendar && this.state.selectedWeekdaysOnCalendar.length > 0) {
            let selectedDates = this.state.selectedWeekdaysOnCalendar.map((selectedWeekday: ITimesheet) => {
                return moment(selectedWeekday.date).format("DD/MM");
            });

            return selectedDates.join(",");
        }

        return "-";
    }

    /**
     * Filter data to be saved  or submitted.
     * @param dataToSaveOrSubmit Dates which need to save or submit.
     */
    private getFilteredDataToBeSavedOrSubmitted(dataToSaveOrSubmit: IUserTimesheet[]) {
        let filteredData: IUserTimesheet[] = [];

        if (dataToSaveOrSubmit) {
            dataToSaveOrSubmit.forEach(timesheet => {
                if (timesheet.projectDetails) {
                    timesheet.projectDetails.forEach(project => {
                        if (project.timesheetDetails && project.timesheetDetails.length > 0) {
                            filteredData.push(timesheet);
                        }
                    });
                }
            });
        }

        return filteredData;
    }

    // The event handler called when screen size get changed
    private onScreenResize() {
        this.setState({ isMobileView: window.outerWidth <= Constants.maxWidthForMobileView });
    }

    /**
     * The event handler called when duplicate efforts request get submitted.
     * @param selectedWeekdaysToDuplicate The selected weekdays to duplicate efforts.
     */
    private async onEffortsDuplicated(selectedWeekdaysToDuplicate: ITimesheet[]) {
        if (selectedWeekdaysToDuplicate && selectedWeekdaysToDuplicate.length) {
            this.setState({ selectedWeekdaysOnCalendar: [] });

            let timesheetData: IUserTimesheet[] = this.state.timesheetDataForCalendar
                && this.state.timesheetDataForCalendar.length > 0 ? cloneDeep(this.state.timesheetDataForCalendar) : [];

            let sourceDateTimesheet = timesheetData.find((timesheet: IUserTimesheet) =>
                timesheet.timesheetDate.valueOf() === this.state.selectedDateInCalendar.date.valueOf());

            if (!sourceDateTimesheet || !sourceDateTimesheet.projectDetails || sourceDateTimesheet.projectDetails.length === 0) {
                this.setState((prevState: IFillTimesheetState) => ({
                    isDuplicatingEfforts: false,
                    notification: { id: prevState.notification.id + 1, message: this.localize("duplicateEffortsNoSourceProjectsError"), type: ActivityStatus.Error }
                }));

                return;
            }

            let targetDates = selectedWeekdaysToDuplicate.map((userTimesheet: ITimesheet) => userTimesheet.date);
            let sourceDateTotalEfforts = getTotalEfforts(sourceDateTimesheet);

            if (this.isDuplicateEffortsExceedWeeklyLimit(sourceDateTotalEfforts, targetDates)) {
                return;
            }

            this.setState({ isDuplicatingEfforts: true });

            let apiResponse = await duplicateEffortsAsync(this.state.selectedDateInCalendar.date, targetDates, moment().toDate(), this.handleTokenAccessFailure);

            if (apiResponse.status === StatusCodes.OK) {
                this.getUserTimesheetsAsync().finally(() => {
                    this.setState((prevState: IFillTimesheetState) => ({
                        isDuplicatingEfforts: false,
                        notification: { id: prevState.notification.id + 1, message: this.localize("TimesheetSaveSuccessfulMessage"), type: ActivityStatus.Success }
                    }));
                });
            }
            else {
                this.setState((prevState: IFillTimesheetState) => ({
                    isDuplicatingEfforts: false,
                    notification: { id: prevState.notification.id + 1, message: this.localize("duplicateEffortsFailedError"), type: ActivityStatus.Error }
                }));
            }
        }
        else {
            this.setState((prevState: IFillTimesheetState) => ({
                notification: { id: prevState.notification.id + 1, message: this.localize("duplicateEffortsFailedError"), type: ActivityStatus.Error }
            }));
        }
    }

    /**
     * Event handler called when toggle 'Duplicate efforts'
     * @param isInEditMode Whether 'Duplicate efforts' is in-progress
     */
    private onCalendarEditModeChange(isInEditMode: boolean) {
        this.setState({ isCalendarInEditMode: isInEditMode });
    }

    /**
     * Event handler called when expand or collapse project details
     * @param projectId The project Id of which state needs to change.
     * @param timesheetDate The timesheet date.
     */
    private onProjectExpandedStateChange(projectId: string, timesheetDate: Date | undefined) {
        if (projectId && timesheetDate) {
            let timesheetDataForProjects: IUserTimesheet[] = this.state.timesheetDataForProjects
                && this.state.timesheetDataForProjects.length > 0 ? cloneDeep(this.state.timesheetDataForProjects) : [];

            timesheetDataForProjects.forEach((timesheet: IUserTimesheet) => {
                if (timesheet && timesheet.projectDetails) {
                    let project = timesheet.projectDetails.find((project: IProjectDetails) =>
                        project.id === projectId && timesheet.timesheetDate.valueOf() === timesheetDate.valueOf());

                    if (project) {
                        project.isProjectViewExpanded = !project.isProjectViewExpanded;
                        this.setState({ timesheetDataForProjects });
                        return;
                    }
                }
            });
        }
    }

    /**
     * Event handler called when the efforts for particular task gets changed
     * @param timesheetDate The timesheet date.
     * @param projectId The project Id.
     * @param taskAtIndex The index in array of which task details to be updated.
     * @param updatedEfforts The updated hours when efforts changed.
     */
    private onTaskEffortChange(timesheetDate: Date, projectId: string, taskAtIndex: number, updatedEfforts: string) {
        let timesheetDataForProjects: IUserTimesheet[] = this.state.timesheetDataForProjects
            && this.state.timesheetDataForProjects.length > 0 ? cloneDeep(this.state.timesheetDataForProjects) : [];

        let timesheetDataForSelectedDate = timesheetDataForProjects.find((timesheet: IUserTimesheet) =>
            timesheet.timesheetDate.valueOf() === timesheetDate.valueOf());

        if (timesheetDataForSelectedDate && timesheetDataForSelectedDate.projectDetails) {
            let projectDetails = timesheetDataForSelectedDate.projectDetails.find((project: IProjectDetails) =>
                project.id === projectId);

            if (projectDetails && projectDetails.timesheetDetails && taskAtIndex > -1 && taskAtIndex < projectDetails.timesheetDetails.length) {
                let timesheetDetails = projectDetails.timesheetDetails[taskAtIndex];

                if (timesheetDetails) {
                    timesheetDetails.hours = Number(updatedEfforts);

                    let dataToSaveOrSubmit: IUserTimesheet[] = this.state.dataToSaveOrSubmit
                        && this.state.dataToSaveOrSubmit.length > 0 ? cloneDeep(this.state.dataToSaveOrSubmit) : [];

                    let timesheetAtIndex = dataToSaveOrSubmit.findIndex(timesheet =>
                        timesheet.timesheetDate.valueOf() === timesheetDate.valueOf());

                    if (timesheetAtIndex > -1) {
                        dataToSaveOrSubmit[timesheetAtIndex] = timesheetDataForSelectedDate;
                    }
                    else {
                        dataToSaveOrSubmit.push(timesheetDataForSelectedDate!);
                    }

                    this.setState({ timesheetDataForProjects, dataToSaveOrSubmit });
                }
            }
        }
    }

    // Event handler called when submitting timesheet
    private async onSubmitTimesheet() {
        if (this.isTimesheetExceededEffortsLimit(this.state.timesheetDataForProjects)) {
            return false;
        }

        let dataToSaveOrSubmit = this.state.dataToSaveOrSubmit
            && this.state.dataToSaveOrSubmit.length > 0 ? cloneDeep(this.state.dataToSaveOrSubmit) : [];

        this.setState({ isSavingTimesheet: true, isSubmittingTimesheet: true });

        if (dataToSaveOrSubmit.length === 0) {
            var apiResponse = await submitTimesheetAsync(this.handleTokenAccessFailure);

            if (apiResponse.status === StatusCodes.OK) {
                this.getUserTimesheetsAsync().finally(() => {
                    this.setState((prevState: IFillTimesheetState) => ({
                        isSubmittingTimesheet: false,
                        isSavingTimesheet: false,
                        notification: { id: prevState.notification.id + 1, message: this.localize("TimesheetSubmitSuccessfulMessage"), type: ActivityStatus.Success }
                    }));
                });
            }
            else {
                this.setState((prevState: IFillTimesheetState) => ({
                    isSubmittingTimesheet: false,
                    isSavingTimesheet: false,
                    notification: { id: prevState.notification.id + 1, message: this.localize("TimesheetSubmitErrorMessage"), type: ActivityStatus.Error }
                }));
            }
        }
        else {
            let apiResponse = await saveTimesheetAsync(dataToSaveOrSubmit, moment().toDate(), this.handleTokenAccessFailure);

            if (apiResponse.status === StatusCodes.OK) {
                this.setState((prevState: IFillTimesheetState) => ({
                    dataToSaveOrSubmit: [],
                    notification: { id: prevState.notification.id + 1, message: this.localize("TimesheetSaveSuccessfulMessage"), type: ActivityStatus.Success }
                }));

                apiResponse = await submitTimesheetAsync(this.handleTokenAccessFailure);

                if (apiResponse.status === StatusCodes.OK) {
                    this.getUserTimesheetsAsync().finally(() => {
                        this.setState((prevState: IFillTimesheetState) => ({
                            isSubmittingTimesheet: false,
                            isSavingTimesheet: false,
                            notification: { id: prevState.notification.id + 1, message: this.localize("TimesheetSubmitSuccessfulMessage"), type: ActivityStatus.Success }
                        }));
                    });
                }
                else {
                    this.setState((prevState: IFillTimesheetState) => ({
                        isSubmittingTimesheet: false,
                        isSavingTimesheet: false,
                        notification: { id: prevState.notification.id + 1, message: this.localize("TimesheetSubmitErrorMessage"), type: ActivityStatus.Error }
                    }));
                }
            }
            else {
                this.setState((prevState: IFillTimesheetState) => ({
                    isSavingTimesheet: false,
                    isSubmittingTimesheet: false,
                    notification: { id: prevState.notification.id + 1, message: this.localize("TimesheetSaveErrorMessage"), type: ActivityStatus.Error }
                }));
            }
        }
    }

    /**
     * Event handler called when selected date changed on calendar
     * @param previousSelectedDate The previous date selected before change.
     * @param selectedDate The current selected date in calendar.
     */
    private onCalendarActiveDateChange(previousSelectedDate: Date, selectedDate: ITimesheet, isTimesheetFrozen: boolean) {
        let userTimesheets: IUserTimesheet[] = this.state.timesheetDataForProjects ? cloneDeep(this.state.timesheetDataForProjects) : [];

        let timesheetOfPreviousSelectedDate = userTimesheets.find((timesheet: IUserTimesheet) =>
            timesheet.timesheetDate.valueOf() === previousSelectedDate.valueOf());

        if (timesheetOfPreviousSelectedDate && timesheetOfPreviousSelectedDate.projectDetails) {
            let project = timesheetOfPreviousSelectedDate.projectDetails.find((project: IProjectDetails) =>
                project.isAddNewTaskActivated);

            if (project && project.timesheetDetails) {
                let newTaskAtIndex = project.timesheetDetails.findIndex((timesheet: ITimesheetDetails) =>
                    timesheet.taskId === Guid.EMPTY);

                if (newTaskAtIndex > -1) {
                    project.isAddNewTaskActivated = false;
                    project.timesheetDetails.splice(newTaskAtIndex, 1);
                }
            }
        }

        this.setState({
            selectedDateInCalendar: selectedDate,
            timesheetDataForProjects: userTimesheets,
            isTimesheetDisabled: isTimesheetFrozen
        });
    }

    // Event handler called when click on save timesheet.
    private async onSaveTimesheet() {
        if (this.isTimesheetExceededEffortsLimit(this.state.timesheetDataForProjects)) {
            return false;
        }

        let dataToSaveOrSubmit = this.state.dataToSaveOrSubmit
            && this.state.dataToSaveOrSubmit.length > 0 ? cloneDeep(this.state.dataToSaveOrSubmit) : [];

        if (dataToSaveOrSubmit.length === 0) {
            return false;
        }

        this.setState({ isSavingTimesheet: true });
        let apiResponse = await saveTimesheetAsync(dataToSaveOrSubmit, moment().toDate(), this.handleTokenAccessFailure);

        if (apiResponse.status === StatusCodes.OK) {
            this.getUserTimesheetsAsync().finally(() => {
                this.setState((prevState: IFillTimesheetState) => ({
                    isSavingTimesheet: false,
                    notification: { id: prevState.notification.id + 1, message: this.localize("TimesheetSaveSuccessfulMessage"), type: ActivityStatus.Success }
                }));
            });
        }
        else {
            this.setState((prevState: IFillTimesheetState) => ({
                isSavingTimesheet: false,
                notification: { id: prevState.notification.id + 1, message: this.localize("TimesheetSaveErrorMessage"), type: ActivityStatus.Error }
            }));
        }
    }

    /**
     * Event handler called when week gets changed in calendar.
     * @param weekNumber The week that is rendered in calendar.
     */
    private onWeekChange(weekNumber: number) {
        this.setState({ renderedWeek: weekNumber }, this.getUserTimesheetsAsync);
    }

    /**
     * Event handler called when deleting a task
     * @param timesheetDate The timesheet date selected on calendar.
     * @param projectId The project Id of which task need to be deleted.
     * @param taskAtIndex The index in array of which task to be deleted.
     */
    private async onDeleteTask(timesheetDate: Date, projectId: string, taskAtIndex: number) {
        if (projectId && taskAtIndex > -1) {
            let userTimesheets: IUserTimesheet[] = this.state.timesheetDataForProjects ? cloneDeep(this.state.timesheetDataForProjects) : [];

            let timesheetOfWeekday = userTimesheets.find((timesheet: IUserTimesheet) => {
                return timesheet.timesheetDate.valueOf() === timesheetDate.valueOf();
            });

            if (timesheetOfWeekday && timesheetOfWeekday.projectDetails) {
                let projectDetails = timesheetOfWeekday.projectDetails.find((project: IProjectDetails) =>
                    project.id === projectId);

                if (projectDetails && projectDetails.timesheetDetails && taskAtIndex < projectDetails.timesheetDetails.length) {
                    projectDetails.timesheetDetails[taskAtIndex].isDeleteTaskInProgress = true;
                    this.setState({ timesheetDataForProjects: userTimesheets, isDeleteTaskInProgress: true });

                    let apiReponse = await deleteMemberTaskAsync(projectId, projectDetails.timesheetDetails[taskAtIndex].taskId, this.handleTokenAccessFailure);

                    if (apiReponse.status === StatusCodes.OK) {
                        projectDetails.timesheetDetails[taskAtIndex].isDeleteTaskInProgress = false;

                        let dataToSaveOrSubmit: IUserTimesheet[] = this.state.dataToSaveOrSubmit ? cloneDeep(this.state.dataToSaveOrSubmit) : [];
                        this.copyOrDeleteTask(projectDetails.timesheetDetails[taskAtIndex], projectDetails.id, dataToSaveOrSubmit, true);
                        var filteredDataToBeSavedOrSubmitted: IUserTimesheet[] = this.getFilteredDataToBeSavedOrSubmitted(dataToSaveOrSubmit);

                        this.copyOrDeleteTask(projectDetails.timesheetDetails[taskAtIndex], projectDetails.id, userTimesheets, true);

                        this.setState((prevState: IFillTimesheetState) => ({
                            timesheetDataForProjects: userTimesheets,
                            timesheetDataForCalendar: userTimesheets,
                            isDeleteTaskInProgress: false,
                            dataToSaveOrSubmit: filteredDataToBeSavedOrSubmitted,
                            notification: { id: prevState.notification.id + 1, message: this.localize("fillTimesheetDeleteTaskSuccess"), type: ActivityStatus.Success }
                        }));
                    }
                    else {
                        projectDetails.timesheetDetails[taskAtIndex].isDeleteTaskInProgress = false;

                        this.setState((prevState: IFillTimesheetState) => ({
                            timesheetDataForProjects: userTimesheets,
                            isDeleteTaskInProgress: false,
                            notification: { id: prevState.notification.id + 1, message: this.localize("SomethingWentWrongMessage"), type: ActivityStatus.Error }
                        }));
                    }
                }
            }
        }
    }

    /**
     * Event handler called when requested to add new task.
     * @param timesheetDate The date for which new task to be added.
     * @param projectId The project Id for which new task to be added.
     */
    private onRequestToAddNewTask(timesheetDate: Date, projectId: string) {
        let userTimesheets: IUserTimesheet[] = this.state.timesheetDataForProjects ? cloneDeep(this.state.timesheetDataForProjects) : [];

        let userTimesheet = userTimesheets.find((userTimesheet: IUserTimesheet) =>
            userTimesheet.timesheetDate.valueOf() === timesheetDate.valueOf());

        if (userTimesheet && userTimesheet.projectDetails) {
            let project = userTimesheet.projectDetails.find((project: IProjectDetails) => project.id === projectId);

            if (project) {
                project.isAddNewTaskActivated = true;

                let timesheets = project.timesheetDetails ? project.timesheetDetails : [];

                timesheets.push({
                    taskId: Guid.createEmpty().toString(),
                    taskTitle: "",
                    hours: 0,
                    status: TimesheetStatus.None,
                    managerComments: "",
                    isAddedByMember: true,
                    isDeleteTaskInProgress: false,
                    startDate: this.state.selectedDateInCalendar.date,
                    endDate: this.state.selectedDateInCalendar.date
                });

                project.timesheetDetails = timesheets;
                this.setState({ timesheetDataForProjects: userTimesheets });
            }
        }
    }

    /**
     * Event handler called when task name get changed for new task.
     * @param timesheetDate The date of which new task name to be updated.
     * @param event The input event details.
     * @param projectId The project Id of which new task name to be updated.
     */
    private onNewTaskNameChange(timesheetDate: Date, event: any, projectId: string) {
        let userTimesheets: IUserTimesheet[] = this.state.timesheetDataForProjects ? cloneDeep(this.state.timesheetDataForProjects) : [];

        let userTimesheet = userTimesheets.find((userTimesheet: IUserTimesheet) =>
            userTimesheet.timesheetDate.valueOf() === timesheetDate.valueOf());

        if (userTimesheet && userTimesheet.projectDetails) {
            let project = userTimesheet.projectDetails.find((project: IProjectDetails) => project.id === projectId);

            if (project) {
                let timesheets = project.timesheetDetails ? project.timesheetDetails : [];
                let timesheet = timesheets.find((timesheet: ITimesheetDetails) => timesheet.taskId === Guid.EMPTY);

                if (timesheet) {
                    timesheet.taskTitle = event.target.value;

                    project.timesheetDetails = timesheets;
                    this.setState({ timesheetDataForProjects: userTimesheets });
                }
            }
        }
    }

    /**
     * Event handler called when new task get submitted.
     * @param timesheetDate The date of which new task name to be submitted.
     * @param projectId The project Id in which task to be created.
     */
    private async onNewTaskSubmit(timesheetDate: Date, projectId: string) {
        let userTimesheets: IUserTimesheet[] = this.state.timesheetDataForProjects ? cloneDeep(this.state.timesheetDataForProjects) : [];

        let userTimesheet = userTimesheets.find((userTimesheet: IUserTimesheet) =>
            userTimesheet.timesheetDate.valueOf() === timesheetDate.valueOf());

        if (userTimesheet && userTimesheet.projectDetails) {
            let project = userTimesheet.projectDetails.find((project: IProjectDetails) => project.id === projectId);

            if (project) {
                let timesheets = project.timesheetDetails ? project.timesheetDetails : [];
                let timesheetAtIndex = timesheets.findIndex((timesheet: ITimesheetDetails) => timesheet.taskId === Guid.EMPTY);

                if (timesheetAtIndex > -1) {
                    if (timesheets[timesheetAtIndex].taskTitle.trim().length <= 0) {
                        this.setState((prevState: IFillTimesheetState) => ({
                            notification: { id: prevState.notification.id + 1, message: this.localize("fillTimesheetTaskTitleError"), type: ActivityStatus.Error }
                        }));

                        return;
                    }

                    if (timesheets[timesheetAtIndex].endDate.valueOf() > project.endDate.valueOf()) {
                        this.setState((prevState: IFillTimesheetState) => ({
                            notification: { id: prevState.notification.id + 1, message: this.localize("fillTimesheetTaskEndDateError", { projectEndDate: moment(project?.endDate).format("YYYY-MM-DD") }), type: ActivityStatus.Error }
                        }));

                        return;
                    }

                    project.isAddNewTaskInProgress = true;
                    this.setState({ timesheetDataForProjects: userTimesheets, isAddTaskInProgress: true });

                    let apiResponse = await addMemberTaskAsync(timesheets[timesheetAtIndex], projectId, this.handleTokenAccessFailure);

                    if (apiResponse.status === StatusCodes.OK) {
                        project.isAddNewTaskInProgress = false;

                        let createdTaskDetails = apiResponse.data as ITask;

                        if (createdTaskDetails && createdTaskDetails.id !== Guid.EMPTY) {

                            project.isAddNewTaskActivated = false;
                            timesheets[timesheetAtIndex].taskId = createdTaskDetails.id;

                            project.timesheetDetails = timesheets;

                            this.copyOrDeleteTask(timesheets[timesheetAtIndex], projectId, userTimesheets);

                            this.setState((prevState: IFillTimesheetState) => ({
                                timesheetDataForProjects: userTimesheets,
                                isAddTaskInProgress: false,
                                notification: { id: prevState.notification.id + 1, message: this.localize("fillTimesheetAddTaskSuccess"), type: ActivityStatus.Success }
                            }));
                        }
                        else {
                            this.setState((prevState: IFillTimesheetState) => ({
                                timesheetDataForProjects: userTimesheets,
                                isAddTaskInProgress: false,
                                notification: { id: prevState.notification.id + 1, message: this.localize("fillTimesheetAddTaskFailed"), type: ActivityStatus.Error }
                            }));
                        }
                    }
                    else {
                        project.isAddNewTaskInProgress = false;

                        this.setState((prevState: IFillTimesheetState) => ({
                            timesheetDataForProjects: userTimesheets,
                            isAddTaskInProgress: false,
                            notification: { id: prevState.notification.id + 1, message: this.localize("SomethingWentWrongMessage"), type: ActivityStatus.Error }
                        }));
                    }
                }
            }
        }
    }

    /**
     * Event handler called when adding a new task to be canceled.
     * @param timesheetDate The date of which adding new task to be canceled.
     * @param projectId The project Id of which adding new task to be canceled.
     */
    private onCancelCreateNewTask(timesheetDate: Date, projectId: string) {
        let userTimesheets: IUserTimesheet[] = this.state.timesheetDataForProjects ? cloneDeep(this.state.timesheetDataForProjects) : [];

        let userTimesheet = userTimesheets.find((userTimesheet: IUserTimesheet) =>
            userTimesheet.timesheetDate.valueOf() === timesheetDate.valueOf());

        if (userTimesheet && userTimesheet.projectDetails) {
            let project = userTimesheet.projectDetails.find((project: IProjectDetails) => project.id === projectId);

            if (project) {
                project.isAddNewTaskActivated = false;

                let timesheets = project.timesheetDetails ? project.timesheetDetails : [];

                if (timesheets.length > 0) {
                    timesheets.pop();
                    project.timesheetDetails = timesheets;

                    this.setState({ timesheetDataForProjects: userTimesheets });
                }
            }
        }
    }

    /**
     * Event handler called when new task end date changed.
     * @param timesheetDate The date for which task end date to change.
     * @param projectId The project Id of a task.
     * @param selectedDate The changed date.
     */
    private onNewTaskEndDateChange(timesheetDate: Date, projectId: string, selectedDate: Date) {
        let userTimesheets: IUserTimesheet[] = this.state.timesheetDataForProjects ? cloneDeep(this.state.timesheetDataForProjects) : [];

        let userTimesheet = userTimesheets.find((userTimesheet: IUserTimesheet) =>
            userTimesheet.timesheetDate.valueOf() === timesheetDate.valueOf());

        if (userTimesheet && userTimesheet.projectDetails) {
            let project = userTimesheet.projectDetails.find((project: IProjectDetails) => project.id === projectId);

            if (project) {
                let timesheets = project.timesheetDetails ? project.timesheetDetails : [];
                let timesheet = timesheets.find((timesheet: ITimesheetDetails) => timesheet.taskId === Guid.EMPTY);

                if (timesheet) {
                    timesheet.startDate = timesheetDate;
                    timesheet.endDate = moment(selectedDate).startOf('day').toDate();

                    this.setState({ timesheetDataForProjects: userTimesheets });
                }
            }
        }
    }

    /**
     * Event handler called on selected dates on calendar get changed.
     * @param selectedWeekdays The selected weekdays on calendar.
     */
    private onSelectedDatesChange(selectedWeekdays: ITimesheet[]) {
        this.setState({ selectedWeekdaysOnCalendar: selectedWeekdays });
    }

    // Renders mobile view
    private renderMobileView() {
        return (
            <Flex className="fill-timesheet" column>
                <Calendar
                    isManagerView={false}
                    isMobile={this.state.isMobileView}
                    isDuplicatingEfforts={this.state.isDuplicatingEfforts}
                    isDisabled={this.state.isSavingTimesheet || this.state.isSubmittingTimesheet}
                    isLoading={this.state.isUserTimesheetsLoading}
                    timesheetData={this.state.timesheetDataForCalendar}
                    timesheetFreezeDayOfMonth={this.applicationSettings.timesheetFreezeDayOfMonth}
                    onEffortsDuplicated={this.onEffortsDuplicated}
                    onCalendarEditModeChange={this.onCalendarEditModeChange}
                    onCalendarActiveDateChange={this.onCalendarActiveDateChange}
                    onWeekChange={this.onWeekChange}
                    onSelectedDatesChange={this.onSelectedDatesChange}
                />
                <Projects
                    isMobile={this.state.isMobileView}
                    isDisabled={this.isControlDisabled()}
                    selectedCalendarDate={this.state.selectedDateInCalendar.date}
                    timesheetData={this.state.timesheetDataForProjects}
                    onProjectExpandedStateChange={this.onProjectExpandedStateChange}
                    onTaskEffortChange={this.onTaskEffortChange}
                    onRequestToAddNewTask={this.onRequestToAddNewTask}
                    onNewTaskNameChange={this.onNewTaskNameChange}
                    onNewTaskSubmit={this.onNewTaskSubmit}
                    onCancelCreateNewTask={this.onCancelCreateNewTask}
                    onDeleteTask={this.onDeleteTask}
                    onNewTaskEndDateChange={this.onNewTaskEndDateChange}
                />
                <Flex.Item push>
                    <Flex hAlign="end" vAlign="center" gap="gap.small">
                        <Button
                            disabled={this.isControlDisabled() || !this.state.dataToSaveOrSubmit || this.state.dataToSaveOrSubmit.length === 0}
                            loading={this.state.isSavingTimesheet}
                            content={this.localize("saveButtonText")}
                            onClick={this.onSaveTimesheet} />
                        <Dialog
                            design={{ width: "30rem" }}
                            header={<Text content={this.localize("submitTimesheetToManagerMobileDialogHeader")} weight="semibold" />}
                            content={this.localize("submitTimesheetToManagerMobileDialogContent")}
                            cancelButton={this.localize("cancelButtonLabel")}
                            confirmButton={this.localize("confirmationSubmitButtonLabel")}
                            onConfirm={this.onSubmitTimesheet}
                            trigger={
                                <Button
                                    primary
                                    disabled={this.isControlDisabled() || !this.areTimesheetsAvailableToSubmit()}
                                    loading={this.state.isSubmittingTimesheet}
                                    content={this.localize("submitButtonText")} />
                            }
                        />
                    </Flex>
                </Flex.Item>
            </Flex>
        );
    }

    // Renders date details for selected date on calendar
    private renderCalendarDateInfo() {
        if (this.state.isCalendarInEditMode) {
            return (
                <Flex className="half-width" column gap="gap.small">
                    <Text content={this.localize("duplicateEffortsLabel")} size="larger" />
                    <Flex vAlign="center" gap="gap.large">
                        <Flex className="half-width" column gap="gap.smaller">
                            <Text content={this.localize("duplicateEffortsSourceDateLabel")} weight="semibold" />
                            <Text content={this.localize("duplicateEffortsSourceHoursLabel")} weight="semibold" />
                            <Text content={this.localize("duplicateEffortsSelectedDaysLabel")} weight="semibold" />
                        </Flex>
                        <Flex className="half-width" column gap="gap.smaller">
                            <Text content={moment(this.state.selectedDateInCalendar.date).format("DD/MM/YYYY")} />
                            <Text content={this.state.selectedDateInCalendar ? ("0" + this.state.selectedDateInCalendar.hours).slice(-2) : "00"} />
                            <Text truncated title={this.getSelectedDatesToDuplicateEfforts()} content={this.getSelectedDatesToDuplicateEfforts()} />
                        </Flex>
                    </Flex>
                </Flex>
            );
        }

        return (
            <Flex className="half-width" column gap="gap.small">
                <Text content={moment(this.state.selectedDateInCalendar.date).format("MMMM DD, YYYY")} size="larger" />
                <Flex vAlign="center" gap="gap.large">
                    <Flex className="half-width" column gap="gap.smaller">
                        <Text content={this.localize("fillTimesheetTotalHoursLabel")} weight="semibold" />
                        <Text content={this.localize("fillTimesheetStatusLabel")} weight="semibold" />
                        <Text content={this.localize("fillTimesheetManagerCommentsLabel")} weight="semibold" />
                    </Flex>
                    <Flex className="half-width" column gap="gap.smaller">
                        <Text content={this.state.selectedDateInCalendar ? ("0" + this.state.selectedDateInCalendar.hours).slice(-2) : "00"} />
                        <Text data-testid="selected-date-timesheet-status" content={this.getTimesheetStatus()} />
                        <Text truncated title={this.getManagerComments()} content={this.getManagerComments()} />
                    </Flex>
                </Flex>
            </Flex>
        );
    }

    // Renders desktop view
    private renderDesktopView() {
        return (
            <Flex className="fill-timesheet timesheet-web" column>
                <Flex gap="gap.large">
                    <Flex.Item className="half-width">
                        <Calendar
                            isManagerView={false}
                            isMobile={this.state.isMobileView}
                            isDuplicatingEfforts={this.state.isDuplicatingEfforts}
                            isDisabled={this.state.isSavingTimesheet || this.state.isSubmittingTimesheet}
                            isLoading={this.state.isUserTimesheetsLoading}
                            timesheetData={this.state.timesheetDataForCalendar}
                            timesheetFreezeDayOfMonth={this.applicationSettings.timesheetFreezeDayOfMonth}
                            onEffortsDuplicated={this.onEffortsDuplicated}
                            onCalendarEditModeChange={this.onCalendarEditModeChange}
                            onCalendarActiveDateChange={this.onCalendarActiveDateChange}
                            onWeekChange={this.onWeekChange}
                            onSelectedDatesChange={this.onSelectedDatesChange}
                        />
                    </Flex.Item>
                    {this.renderCalendarDateInfo()}
                </Flex>
                <Projects
                    isMobile={this.state.isMobileView}
                    isDisabled={this.isControlDisabled()}
                    selectedCalendarDate={this.state.selectedDateInCalendar.date}
                    timesheetData={this.state.timesheetDataForProjects}
                    onProjectExpandedStateChange={this.onProjectExpandedStateChange}
                    onTaskEffortChange={this.onTaskEffortChange}
                    onRequestToAddNewTask={this.onRequestToAddNewTask}
                    onNewTaskNameChange={this.onNewTaskNameChange}
                    onNewTaskSubmit={this.onNewTaskSubmit}
                    onCancelCreateNewTask={this.onCancelCreateNewTask}
                    onDeleteTask={this.onDeleteTask}
                    onNewTaskEndDateChange={this.onNewTaskEndDateChange}
                />
                <Flex.Item push>
                    <Flex hAlign="end" gap="gap.small">
                        <Button
                            disabled={this.isControlDisabled() || !this.state.dataToSaveOrSubmit || this.state.dataToSaveOrSubmit.length === 0}
                            loading={this.state.isSavingTimesheet}
                            content={this.localize("saveButtonText")}
                            onClick={this.onSaveTimesheet} />
                        <Dialog
                            design={{ width: "60rem" }}
                            header={<Text content={this.localize("submitTimesheetToManagerDesktopDialogHeader")} weight="semibold" />}
                            content={this.localize("submitTimesheetToManagerDesktopDialogContent")}
                            cancelButton={this.localize("cancelButtonLabel")}
                            confirmButton={this.localize("confirmationSubmitButtonLabel")}
                            onConfirm={this.onSubmitTimesheet}
                            trigger={
                                <Button
                                    primary
                                    disabled={this.isControlDisabled() || !this.areTimesheetsAvailableToSubmit()}
                                    loading={this.state.isSubmittingTimesheet}
                                    content={this.localize("submitButtonText")} />
                            }
                        />
                    </Flex>
                </Flex.Item>
            </Flex>
        );
    }

    // Renders component
    render() {
        return (
            <React.Fragment>
                <StatusBar notification={this.state.notification} isMobile={this.state.isMobileView} />
                {this.state.isMobileView ? this.renderMobileView() : this.renderDesktopView()}
            </React.Fragment>
        );
    }
}

export default withTranslation()(withRouter(FillTimesheet));