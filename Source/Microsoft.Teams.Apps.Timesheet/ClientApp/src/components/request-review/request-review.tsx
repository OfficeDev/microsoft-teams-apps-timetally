// <copyright file="request-review.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { IWithContext } from "../../providers/context-provider";
import { Flex, Provider, Text, Button, Avatar, Menu, MenuProps, Checkbox, Table, TextArea, Divider, List, Loader } from '@fluentui/react-northstar';
import { QuestionCircleIcon } from '@fluentui/react-icons-northstar';
import Calendar from "../common/calendar/calendar";
import { TimesheetStatus } from "../../models/timesheet-status";
import IUserTimesheet from "../../models/fill-timesheet/user-timesheet";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { ISubmittedRequest } from "../../models/submitted-request";
import IProjectDetails from "../../models/fill-timesheet/project-details";
import moment from "moment";
import ITimesheetDetails from "../../models/fill-timesheet/timesheet-details";
import ITimesheet from "../../models/timesheet";
import { cloneDeep } from "lodash";
import { approveTimesheetsAsync, rejectTimesheetsAsync } from "../../api/timesheet";
import { getUserTimesheetsAsync, getUserTimesheetsOverviewAsync } from "../../api/users";
import Constants from "../../constants/constants";
import { withRouter, RouteComponentProps } from "react-router-dom";
import { IRequestApproval } from "../../models/request-approval";
import { Guid } from "guid-typescript";

import "./request-review.scss";
import { StatusCodes } from "http-status-codes";

interface IRequestReviewState {
    submittedRequests: ISubmittedRequest[];
    userTimesheet: IUserTimesheet[]
    isLoading: boolean;
    activeTabIndex: number | string | undefined;
    managersComment: string;
    isReasonInputValid: boolean;
    isTimesheetTab: boolean;
    isMobileView: boolean;
    isMobileTaskList: boolean;
    isRejectClick: boolean;
    selectedDateTimesheetStatusForCalendar: ITimesheet;
    selectedWeek: number;
    isViewTimesheet: boolean;
    projectTotalHours: number;
    isApprovingOrRejecting: boolean;
}

interface IRequestReviewParams {
    userId?: string | undefined;
    userName?: string | undefined;
    isMobileView: string;
}

interface IRequestReviewProps extends WithTranslation, IWithContext, RouteComponentProps {
}

// The tab index for 'Registered events' tab 
const RequestsTabIndex: number = 0;

// The tab index for 'Completed events'
const TimesheetTabIndex: number = 1;

// Table's date column width.
const tableDateColumnWidth: string = "20vw";

// Table's hours column width.
const tableHoursColumnWidth: string = "20vw";

// Table's project column width.
const tableProjectsColumnWidth: string = "40vw";

// Renders task module for project utilization
class RequestReview extends React.Component<IRequestReviewProps, IRequestReviewState> {
    readonly localize: TFunction;

    // Table's check-box column design.
    tableCheckboxColumnDesign = { minWidth: Constants.tableCheckboxColumnWidth, maxWidth: Constants.tableCheckboxColumnWidth }

    // Table's date column design.
    tableDateColumnDesign = { minWidth: tableDateColumnWidth, maxWidth: tableDateColumnWidth }

    // Table's hours column design.
    tableHoursColumnDesign = { minWidth: tableHoursColumnWidth, maxWidth: tableHoursColumnWidth }

    // Table's project column design.
    tableProjectsColumnDesign = { minWidth: tableProjectsColumnWidth, maxWidth: tableProjectsColumnWidth }

    params: IRequestReviewParams = {} as IRequestReviewParams;

    /** 
     * Constructor which initializes state.
     */
    constructor(props: any) {
        super(props);
        this.params = this.props.match.params as IRequestReviewParams;

        this.localize = this.props.t;
        this.state = {
            submittedRequests: [],
            userTimesheet: [],
            isLoading: false,
            activeTabIndex: RequestsTabIndex,
            managersComment: "",
            isReasonInputValid: true,
            isMobileView: this.params.isMobileView === "true",
            isMobileTaskList: false,
            isTimesheetTab: false,
            isRejectClick: false,
            selectedDateTimesheetStatusForCalendar: { date: moment().startOf('day').toDate(), hours: 0, status: TimesheetStatus.None },
            selectedWeek: moment().week(),
            isViewTimesheet: false,
            projectTotalHours: 0,
            isApprovingOrRejecting: false,
        };
    }

    /** 
     * Called when component get mounted.
     */
    componentDidMount() {
        this.getSubmittedRequests(this.params.userId!);
        this.getUserTimesheetDetailsAsync(this.state.selectedWeek, this.params.userId!);
    }

    /**
     * Get users submitted timesheet requests details.
     * @param userId The user Id of which timesheets to fetch.
     */
    getSubmittedRequests = async (userId: string) => {
        this.setState({ isLoading: true });
        let response = await getUserTimesheetsAsync(userId, this.handleTokenAccessFailure);
        if (response.status === StatusCodes.OK && response.data) {
            this.setState({ submittedRequests: response.data, isLoading: false });
        } else {
            this.setState({ isLoading: false });
        }
    }

    /**
     * Get timesheet details of user.
     * @param selectedWeek Selected week in calendar.
     * @param userId The user id of which request to fetch.
     */
    getUserTimesheetDetailsAsync = async (selectedWeek: number, userId: string): Promise<any> => {
        let startOfWeekDate = moment().week(selectedWeek).startOf('week').toDate();
        let endOfWeekDate = moment().week(selectedWeek).endOf('week').toDate();
        let isExist = this.state.userTimesheet.filter((userTimesheet: IUserTimesheet) => moment(userTimesheet.timesheetDate).week() === selectedWeek);
        if (isExist.length > 0) {
            return null;
        }
        this.setState({ isLoading: true });
        let response = await getUserTimesheetsOverviewAsync(startOfWeekDate, endOfWeekDate, userId, this.handleTokenAccessFailure);
        if (response.status === StatusCodes.OK && response.data) {
            let userTimesheets: IUserTimesheet[] = response.data;
            userTimesheets.map((userTimesheet: IUserTimesheet) => (
                userTimesheet.timesheetDate = moment(userTimesheet.timesheetDate).startOf('day').toDate()
            ));
            this.setState((prevState: IRequestReviewState) => ({
                userTimesheet: [...prevState.userTimesheet, ...userTimesheets],
                isLoading: false,
            }));
        }
    }

    /**
     * Handles token access failure.
     * @param error Error string.
     */
    handleTokenAccessFailure = (error: string) => {
        this.props.history.push("/signin");
    }

    /**
     * Get total hours of selected date timesheets.
     * @param userTimesheet The user timesheet which hours to calculate.
     */
    getSelectedDateTotalHours = (userTimesheet: IUserTimesheet) => {
        let hours = 0;
        userTimesheet.projectDetails
            .map((projectDetail: IProjectDetails) => projectDetail.timesheetDetails
                .map((timesheet: ITimesheetDetails) => {
                    hours = timesheet.hours + hours;
                }));
        return hours;
    }

    /**
     * Get timesheet status text.
     */
    getTimesheetStatus() {
        switch (this.state.selectedDateTimesheetStatusForCalendar.status) {
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

    /**
    * Function for applying validation on the fields before moving onto send.
    */
    checkIfSendAllowed = () => {
        let eventValidationStatus = { isReasonInputValid: true };

        if (this.state.managersComment.trim().length > Constants.reasonDescriptionMaxLength) {
            eventValidationStatus.isReasonInputValid = false;
        }

        this.setState({
            isReasonInputValid: eventValidationStatus.isReasonInputValid
        });

        return eventValidationStatus.isReasonInputValid;
    }

    /**
     * The event handler called when any timesheet-requests checked state changed.
     * @param submittedRequest The selected submittedRequest details.
     */
    onRequestCheckedChange = (submittedRequest: ISubmittedRequest) => {
        let submittedRequests = cloneDeep(this.state.submittedRequests);
        let checkedRequest = submittedRequests.find((request: ISubmittedRequest) => request.timesheetDate === submittedRequest.timesheetDate)!;
        checkedRequest.isSelected = !checkedRequest.isSelected;
        this.setState({
            submittedRequests
        });
    }

    /**
     * Format date from UTC to user's local time zone.
     * @param submittedRequest pendinfg requests.
     */
    formatDate = (submittedRequest: ISubmittedRequest) => {
        let date = moment.utc(submittedRequest.timesheetDate).local().format("DD");
        let month = moment.utc(submittedRequest.timesheetDate).local().format("MM");
        let year = moment.utc(submittedRequest.timesheetDate).local().format("YYYY");
        return `${date}/${month}/${year}`;
    }

    /** 
     * Get selected timesheet requests.
     */
    getSelectedTimesheetRequests = () => {
        if (this.state.isViewTimesheet && this.state.activeTabIndex === TimesheetTabIndex) {
            return this.state.submittedRequests.filter((submittedRequest: ISubmittedRequest) =>
                moment(submittedRequest.timesheetDate).startOf('day').toDate().valueOf() === moment(this.state.selectedDateTimesheetStatusForCalendar.date).startOf('day').toDate().valueOf()) as ISubmittedRequest[];
        }

        return this.state.submittedRequests.filter((submittedRequest: ISubmittedRequest) => submittedRequest.isSelected) as ISubmittedRequest[];
    }

    /**
     * Event handler when user click reject button.
     */
    onRejectClick = () => {
        let selectedTimesheetRequests = this.getSelectedTimesheetRequests();
        if ((this.state.isViewTimesheet && this.state.activeTabIndex === TimesheetTabIndex) || selectedTimesheetRequests.length > 0) {
            this.setState((prevState: IRequestReviewState) => ({
                isRejectClick: !prevState.isRejectClick
            }));
        }
    }

    /**
     * Invoked when user approve/reject request(s).
     * @param selectedRequests The request(s) of which status to update.
     * @param status The status to update.
     */
    requestUpdateAsync = async (selectedRequests: ISubmittedRequest[], status: TimesheetStatus) => {
        this.setState({ isLoading: true });
        let response: any;
        let requestApproval: IRequestApproval[] = [];

        selectedRequests.map((timesheetRequest: ISubmittedRequest) => {
            timesheetRequest.submittedTimesheetIds.map((timesheetId: Guid) => {
                requestApproval.push({
                    managerComments: status === TimesheetStatus.Approved ? "" : this.state.managersComment,
                    status: TimesheetStatus.Approved,
                    userId: timesheetRequest.userId,
                    timesheetDate: [],
                    timesheetId: timesheetId
                });
            });
        });

        this.setState({ isApprovingOrRejecting: true });
        switch (status) {
            case TimesheetStatus.Approved:
                response = await approveTimesheetsAsync(requestApproval, this.handleTokenAccessFailure);
                break;
            case TimesheetStatus.Rejected:
                response = await rejectTimesheetsAsync(requestApproval, this.handleTokenAccessFailure);
                break;
        }

        if (response.status === StatusCodes.NO_CONTENT) {
            this.setState({ isLoading: false, userTimesheet: [], isApprovingOrRejecting: false });
            this.getUserTimesheetDetailsAsync(this.state.selectedWeek, this.params.userId!);
            return true;
        } else {
            this.setState({ isLoading: false, isApprovingOrRejecting: false });
            return false;
        }
    }

    /**
     * Event handler when user click approve button. 
     */
    onApproveClick = async () => {
        let submittedRequests = cloneDeep(this.state.submittedRequests);
        let selectedTimesheetRequests = this.getSelectedTimesheetRequests();
        if (selectedTimesheetRequests.length > 0) {
            let isUpdated = await this.requestUpdateAsync(selectedTimesheetRequests, TimesheetStatus.Approved);

            if (isUpdated) {
                selectedTimesheetRequests.map((selectedTimesheetRequest: ISubmittedRequest) => {
                    let index = submittedRequests.findIndex(item => item.timesheetDate === selectedTimesheetRequest.timesheetDate);
                    submittedRequests.splice(index, 1);
                });
                this.setState((prevState: IRequestReviewState) => ({
                    submittedRequests,
                }));
            }
        }
    }

    /** 
     * Event handler when user enter text in reject reason text area. 
     */
    handleTextAreaChange = (event: any) => {
        let reasonDescription = event.target.value;
        this.setState({
            managersComment: reasonDescription,
            isReasonInputValid: true,
        });
    }

    /**
     * Event handler when user click send button. 
     */
    onSendClick = async () => {
        var isSendAllowed = await this.checkIfSendAllowed();
        if (isSendAllowed) {
            let submittedRequests = cloneDeep(this.state.submittedRequests);
            let selectedTimesheetRequests = this.getSelectedTimesheetRequests();
            if (selectedTimesheetRequests.length > 0) {
                let isUpdated = await this.requestUpdateAsync(selectedTimesheetRequests, TimesheetStatus.Rejected);
                if (isUpdated) {
                    selectedTimesheetRequests.map((selectedTimesheetRequest: ISubmittedRequest) => {
                        let index = submittedRequests.findIndex(item => item.timesheetDate === selectedTimesheetRequest.timesheetDate);
                        submittedRequests.splice(index, 1);
                    });

                    this.setState((prevState: IRequestReviewState) => ({
                        submittedRequests,
                        isRejectClick: !prevState.isRejectClick,
                    }));
                }
            }
        }
    }

    /** 
     * Event handler called when selected date changed on calendar. 
     */
    onCalendarActiveDateChange = (previousSelectedDate: Date, selectedDate: ITimesheet, isFreeze: boolean) => {
        this.setState({ selectedDateTimesheetStatusForCalendar: selectedDate });
    }

    /** 
     * Event handler called when selected date changed on calendar.
     */
    onCalendarWeekChange = async (selectedWeek: number) => {
        this.setState({ selectedWeek });
        await this.getUserTimesheetDetailsAsync(selectedWeek, this.params.userId!);
    }

    /** 
     * Get called when tab selection change.
     */
    onTabIndexChange = (event: any, tabEventDetails: MenuProps | undefined) => {
        this.setState((prevState: IRequestReviewState) => ({
            activeTabIndex: tabEventDetails?.activeIndex!,
            isViewTimesheet: !prevState.isViewTimesheet
        }));
    }

    /**
     * Event handler when user switch between requests and timesheet 
     */
    onViewTimesheetToggle = () => {
        this.setState((prevState: IRequestReviewState) => ({
            isViewTimesheet: !prevState.isViewTimesheet,
            isRejectClick: false,
            activeTabIndex: prevState.isViewTimesheet ? RequestsTabIndex : TimesheetTabIndex
        }));
    }

    /** 
     *  The event handler called when select all timesheet-requests checked state changed 
     */
    onSelectAllRequestsCheckedChange = () => {
        if (this.state.submittedRequests && this.state.submittedRequests.length > 0) {
            let submittedRequests = cloneDeep(this.state.submittedRequests);
            let selectedTimesheetRequestCount = this.state.submittedRequests.filter((submittedRequest: ISubmittedRequest) => submittedRequest.isSelected)?.length;
            if (selectedTimesheetRequestCount !== this.state.submittedRequests.length) {
                submittedRequests.map((submittedRequest: ISubmittedRequest) => {
                    submittedRequest.isSelected = true;
                });
            }
            else {
                submittedRequests.map((submittedRequest: ISubmittedRequest) => {
                    submittedRequest.isSelected = !submittedRequest.isSelected;
                });
            }

            this.setState({ submittedRequests });
        }
    }

    /**
     * Return error component
     */
    getErrorMessage = () => {
        return (
            <Flex className="manage-timesheet-request-content" gap="gap.small">
                <Flex.Item>
                    <div className="error-container">
                        <QuestionCircleIcon outline color="green" />
                    </div>
                </Flex.Item>
                <Flex.Item grow>
                    <Flex column gap="gap.small" vAlign="stretch">
                        <div>
                            <Text weight="bold" content={this.localize("timesheetRequestNotAvailableHeaderDescription")} /><br />
                        </div>
                    </Flex>
                </Flex.Item>
            </Flex>
        );
    }

    /**
     * Event handler on fetching the validation message for valid name
     */
    getReasonError = () => {
        if (this.state.managersComment.length > Constants.reasonDescriptionMaxLength) {
            return (<Text content={this.localize("reasonMaxCharError")} error />);
        }

        return (<></>);
    }

    /**
     * Gets total hours of project
     * @param timesheetDetails The timesheet details of the project which is used to evaluate total hours.
     */
    getProjectTotalHours = (timesheetDetails: ITimesheetDetails[]) => {
        let hours = 0;
        timesheetDetails.map((timesheetDetail: ITimesheetDetails) => {
            hours = hours + timesheetDetail.hours;
        });
        return hours;
    }

    /**
     * Event handler when timesheet is reject from calendar.
     */
    onRejectFromTimesheetClick = () => {
        if (this.state.selectedDateTimesheetStatusForCalendar.status === TimesheetStatus.Submitted || this.state.selectedDateTimesheetStatusForCalendar.status === TimesheetStatus.Approved) {
            this.setState((prevState: IRequestReviewState) => ({
                isRejectClick: !prevState.isRejectClick
            }));
        }
    }

    /**
     * Render list of timesheets.
     */
    renderTimesheetRequestsList = () => {
        if (this.state.isLoading) {
            return <Loader />;
        }
        let submittedRequests = cloneDeep(this.state.submittedRequests);
        if (submittedRequests?.length > 0) {
            let items: any[] = submittedRequests.map((submittedRequest: ISubmittedRequest, index: number) => {
                return {
                    key: `submittedRequest-${index}`,
                    content:
                        <div>
                            <Flex className="manage-tasks-list-container" vAlign="center" key={index}>
                                <Flex space="between">
                                    <Flex.Item>
                                        <Flex column>
                                            <Text className="title-text" content={`${this.localize("hours", { hourNumber: submittedRequest.totalHours })}, ${moment(submittedRequest.timesheetDate).format("DD MMM YYYY")}`} weight="semibold" />
                                            <Text className="subtitle-text" content={this.getProjectTitles(submittedRequest)} />
                                        </Flex>
                                    </Flex.Item>
                                </Flex>
                                <Flex.Item push>
                                    <Flex vAlign="center" space="between" gap="gap.small">
                                        <Checkbox key={index} checked={submittedRequest.isSelected} onChange={() => this.onRequestCheckedChange(submittedRequest)} />
                                    </Flex>
                                </Flex.Item>
                            </Flex>
                            <Divider />
                        </div>
                };
            }
            );
            return (
                <Flex column>
                    <Flex className="list-header" vAlign="center" padding="padding.medium">
                        <Text className="mobile-title" content={this.localize("requestsTab")} weight="semibold" />
                        <Flex.Item push>
                            <Button text onClick={this.onSelectAllRequestsCheckedChange} content={this.localize("selectAllButtonLabel")} className="mobile-subtitle" />
                        </Flex.Item>
                    </Flex>
                    <div className="list-container">
                        <List items={items} />
                    </div>
                </Flex>
            );
        }
        else {
            return (
                this.getErrorMessage()
            );
        }
    }

    /**
     * Render task details for selected date timesheet request 
     */
    renderTaskDetails = (projectDetail: IProjectDetails,) => {
        return (
            <Flex className={!this.state.isMobileView ? "project-container" : ""} column>
                <Flex className="mobile-title">
                    <Flex.Item>
                        <Text className="mobile-title" content={projectDetail.title} weight="semibold" />
                    </Flex.Item>
                    <Flex.Item push>
                        <Text className="project-title-detail" content={this.localize("hours", { hourNumber: this.getProjectTotalHours(projectDetail.timesheetDetails) })} weight="semibold" />
                    </Flex.Item>
                </Flex>
                <Flex column>
                    {projectDetail.timesheetDetails.map((timesheetDetail: ITimesheetDetails, index: number) => {
                        return (
                            <Flex className="project-task" key={`project-task-${index}`}>
                                <Flex.Item>
                                    <Text className={this.state.isMobileView ? "mobile-subtitle" : "desktop-subtitle"} content={timesheetDetail.taskTitle} />
                                </Flex.Item>
                                <Flex.Item push>
                                    <Text className={this.state.isMobileView ? "mobile-subtitle" : "desktop-subtitle"} content={this.localize("hours", { hourNumber: timesheetDetail.hours })} />
                                </Flex.Item>
                            </Flex>
                        );
                    })}
                </Flex>
            </Flex>
        );
    }

    /** 
     * Render project details of selected date timesheet request
     */
    renderProjectDetails = () => {
        let selectedDateTimesheets = this.state.userTimesheet.filter((userTimesheet: IUserTimesheet) =>
            moment(userTimesheet.timesheetDate).toDate().toDateString() === this.state.selectedDateTimesheetStatusForCalendar.date.toDateString());
        return (
            <Flex column gap="gap.medium" className="projects-detail-container">
                {selectedDateTimesheets.map((userTimesheet: IUserTimesheet) => userTimesheet.projectDetails.map((project: IProjectDetails) => {
                    if (this.getProjectTotalHours(project.timesheetDetails) > 0) {
                        return this.renderTaskDetails(project);
                    }
                }))}
            </Flex>
        );
    }

    /**
     * Render view where manager can give reject reason 
     */
    renderRejectReasonView = () => {
        let selectedTimesheetRequests = this.getSelectedTimesheetRequests();
        if ((selectedTimesheetRequests && selectedTimesheetRequests.length > 0) || (this.state.isViewTimesheet && this.state.activeTabIndex === TimesheetTabIndex)) {
            return (
                <Flex column gap="gap.medium">
                    {!this.state.isViewTimesheet && this.state.activeTabIndex !== TimesheetTabIndex && selectedTimesheetRequests && selectedTimesheetRequests.length > 1 && <Text content={`${this.localize("timesheetRejectMultipleDates")}`} />}
                    {((this.state.isViewTimesheet && this.state.activeTabIndex === TimesheetTabIndex) || (selectedTimesheetRequests &&
                        selectedTimesheetRequests.length === 1)) && <Text content={this.localize("timesheetRejectLabel", { date: (this.state.isViewTimesheet && this.state.activeTabIndex === TimesheetTabIndex) ? moment(this.state.selectedDateTimesheetStatusForCalendar.date).format("DD/MM/YYYY") : this.formatDate(selectedTimesheetRequests[0]) })} />}
                    <Flex column gap="gap.small">
                        <Flex>
                            <Text content={this.localize("reason")} />
                            <Flex.Item push>
                                {this.getReasonError()}
                            </Flex.Item>
                        </Flex>
                        <TextArea
                            fluid
                            className="reason-input"
                            placeholder={this.localize("reasonTextAreaPlaceholder")}
                            value={this.state.managersComment}
                            onChange={this.handleTextAreaChange}
                            maxLength={Constants.reasonDescriptionMaxLength}
                        />
                    </Flex>
                </Flex>
            );
        }
    }

    /**
     * Gets tab menu items 
     */
    renderTabMenuItems = () => {
        return (
            [
                {
                    key: "user-requests",
                    content: `${this.localize("requestsTab")}`
                },
                {
                    key: "user-timesheet",
                    content: `${this.localize("timesheetTab")}`
                },
            ]
        );
    }

    /**
     * Renders footer button 
     */
    renderFooter = () => {
        return (
            <Flex space="between" vAlign="center" className="button-footer">
                <Flex.Item push >
                    <Flex gap="gap.small">
                        {!this.state.isRejectClick && (this.state.activeTabIndex === TimesheetTabIndex || this.state.isViewTimesheet) &&
                            <>
                                <Button loading={this.state.isApprovingOrRejecting} disabled={this.state.selectedDateTimesheetStatusForCalendar.status !== TimesheetStatus.Submitted || this.state.isApprovingOrRejecting} content={this.localize("reject")} onClick={this.onRejectFromTimesheetClick} />
                                <Button loading={this.state.isApprovingOrRejecting} disabled={this.state.selectedDateTimesheetStatusForCalendar.status !== TimesheetStatus.Submitted || this.state.isApprovingOrRejecting} primary content={this.localize("approve")} onClick={this.onApproveClick} />
                            </>
                        }
                        {!this.state.isRejectClick && (this.state.activeTabIndex === RequestsTabIndex || !this.state.isViewTimesheet) &&
                            <>
                                <Button disabled={!(this.getSelectedTimesheetRequests().length > 0) || this.state.isApprovingOrRejecting} content={this.localize("reject")} onClick={this.onRejectClick} />
                                <Button loading={this.state.isApprovingOrRejecting} disabled={!(this.getSelectedTimesheetRequests().length > 0) || this.state.isApprovingOrRejecting} primary content={this.localize("approve")} onClick={this.onApproveClick} />
                            </>
                        }
                        {this.state.isRejectClick && (this.state.activeTabIndex === RequestsTabIndex || this.state.activeTabIndex === TimesheetTabIndex) &&
                            <Flex gap="gap.small">
                                <Button disabled={this.state.isApprovingOrRejecting} content={this.localize("backButtonLabel")} onClick={this.onRejectClick} />
                                <Button loading={this.state.isApprovingOrRejecting} disabled={this.state.isApprovingOrRejecting} primary content={this.localize("sendButtonLabel")} onClick={this.onSendClick} />
                            </Flex>
                        }
                    </Flex>
                </Flex.Item>
            </Flex>
        );
    }

    /**
     * Format project titles to show in column.
     * @param submittedRequest The requests of which project titles to format.
     */
    getProjectTitles = (submittedRequest: ISubmittedRequest) => {
        var titles = "";
        if (submittedRequest.projectTitles.length === 1) {
            return `${submittedRequest.projectTitles[0]}.`;
        }
        else {
            for (var i = 0; i < submittedRequest.projectTitles.length; i++) {
                if (i === 0) {
                    titles = `${submittedRequest.projectTitles[i]}`;
                }
                else if (i === submittedRequest.projectTitles.length - 1) {
                    titles = `${titles}, ${submittedRequest.projectTitles[i]}.`;
                }
                else {
                    titles = `${titles}, ${submittedRequest.projectTitles[i]}`;
                }
            }
            return titles;
        }
    }

    /** 
     * Render requests table.
     */
    renderRequestsTable = () => {
        if (this.state.isLoading) {
            return <Loader />
        }
        let submittedRequests = this.state.submittedRequests;

        if (submittedRequests?.length > 0) {
            const timesheetRequestTableHeaderItems = {
                key: "header",
                items: [
                    {
                        content: "",
                        design: this.tableCheckboxColumnDesign
                    },
                    {
                        content: <Text weight="semibold" className="table-header" content={this.localize("dateTableHeading")} />,
                        design: this.tableDateColumnDesign
                    },
                    {
                        content: <Text weight="semibold" className="table-header" content={this.localize("hoursTableHeading")} />,
                        design: this.tableHoursColumnDesign
                    },
                    {
                        content: <Text weight="semibold" className="table-header" content={this.localize("projectTableHeading")} />,
                        design: this.tableProjectsColumnDesign
                    }
                ]
            };

            let rows = submittedRequests.map((submittedRequest: ISubmittedRequest, index: number) => {
                return {
                    "key": index,
                    "items": [
                        {
                            content: <Checkbox data-tid={`request-checkbox-${index}`} key={index} checked={submittedRequest.isSelected} onChange={() => this.onRequestCheckedChange(submittedRequest)} />,
                            design: this.tableCheckboxColumnDesign
                        },
                        {
                            content: this.formatDate(submittedRequest),
                            title: this.formatDate(submittedRequest),
                            truncateContent: true,
                            design: this.tableDateColumnDesign
                        },
                        {
                            content: submittedRequest.totalHours,
                            title: submittedRequest.totalHours,
                            truncateContent: true,
                            design: this.tableHoursColumnDesign
                        },
                        {
                            content: this.getProjectTitles(submittedRequest),
                            title: this.getProjectTitles(submittedRequest),
                            truncateContent: true,
                            design: this.tableProjectsColumnDesign
                        }
                    ]
                }
            });

            return (
                <div className={"request-table-container"}>
                    <Table data-tid={`request-review-table`} header={timesheetRequestTableHeaderItems} rows={rows} />
                </div>
            );
        }
        else {
            return (
                this.getErrorMessage()
            );
        }
    }

    /** 
     * Renders date details for selected date on calendar 
     */
    renderCalendarDateInfo = () => {
        let selectedDateTimesheets = this.state.selectedDateTimesheetStatusForCalendar;
        if (selectedDateTimesheets) {
            if (!this.state.isMobileView) {
                return (
                    <Flex column gap="gap.small">
                        <Text content={moment(this.state.selectedDateTimesheetStatusForCalendar.date).format("MMMM DD, YYYY")} weight="semibold" />
                        <Flex vAlign="center" gap="gap.large">
                            <Flex column gap="gap.smaller">
                                <Text size="small" content={`${this.localize("timesheetStatusLabel")}:`} />
                                <Text size="small" content={this.getTimesheetStatus()} weight="semibold" />
                            </Flex>
                        </Flex>
                    </Flex>
                );
            }
            else {
                return (
                    <Flex column gap="gap.small">
                        <Text className="mobile-title" content={moment(this.state.selectedDateTimesheetStatusForCalendar.date).format("MMMM DD, YYYY")} weight="semibold" />
                        <Flex vAlign="center">
                            <Text className={this.state.isMobileView ? "mobile-subtitle" : "desktop-subtitle"} content={`${this.localize("timesheetStatusLabel")}:`} />
                            <Flex.Item push>
                                <Text className={this.state.isMobileView ? "mobile-subtitle" : "desktop-subtitle"} content={this.getTimesheetStatus()} />
                            </Flex.Item>
                        </Flex>
                    </Flex>
                );
            }
        }
    }

    /**
     * Render mobile view.
     */
    renderMobileView = () => {
        return (
            <Flex vAlign="center" gap="gap.large" className="review-request-mobile-view" column>
                <Flex vAlign="center">
                    <Avatar size="larger" className="user-avatar" name={this.params.userName} />
                    <Flex vAlign="center" className="user-details" column>
                        <Flex vAlign="center">
                            <Text className="user-name" content={this.params.userName} weight="semibold" />
                            <Flex.Item push>
                                <Button primary text content={this.state.isViewTimesheet ? this.localize("viewRequestsButtonLabel") : this.localize("viewTimesheetButtonLabel")} onClick={this.onViewTimesheetToggle} />
                            </Flex.Item>
                        </Flex>
                        <Text className="user-subtitle" content="" />
                        <Text className="user-subtitle" content="" />
                    </Flex>
                </Flex>
                {!this.state.isViewTimesheet && !this.state.isRejectClick && this.renderTimesheetRequestsList()}
                {this.state.isRejectClick && this.renderRejectReasonView()}
                {this.state.isViewTimesheet && !this.state.isRejectClick &&
                    <Flex column>
                        <div className="calendar-container">
                            <Calendar
                                isDisabled={true}
                                isDuplicatingEfforts={false}
                                onCalendarEditModeChange={() => void 0}
                                onEffortsDuplicated={() => void 0}
                                onWeekChange={this.onCalendarWeekChange}
                                isMobile={this.state.isMobileView}
                                isManagerView={true}
                                timesheetData={this.state.userTimesheet}
                                onCalendarActiveDateChange={this.onCalendarActiveDateChange}
                                isLoading={this.state.isLoading}
                            />
                        </div>
                        {this.renderCalendarDateInfo()}
                        {this.renderProjectDetails()}
                    </Flex>
                }
                {this.renderFooter()}
            </Flex>
        );
    }

    /**
     * Render desktop view.
     */
    renderDesktopView = () => {
        return (
            <div className="page-content">
                <Flex vAlign="center" gap="gap.large" column>
                    <Flex vAlign="center" >
                        <Avatar size="larger" className="user-avatar" name={this.params.userName} />
                        <Flex.Item>
                            <Flex vAlign="center" column>
                                <Text className="user-name" content={this.params.userName} weight="semibold" />
                                <Text className="user-subtitle" content="" />
                                <Text className="user-subtitle" content="" />
                            </Flex>
                        </Flex.Item>
                    </Flex>
                    {!this.state.isRejectClick &&
                        <Menu
                            underlined
                            primary
                            items={this.renderTabMenuItems()}
                            defaultActiveIndex={RequestsTabIndex}
                            activeIndex={this.state.activeTabIndex}
                            onActiveIndexChange={this.onTabIndexChange}
                            data-tid={`view-timesheet-menu`}
                        />
                    }
                    {this.state.activeTabIndex === RequestsTabIndex && !this.state.isRejectClick && this.renderRequestsTable()}
                    {this.state.isRejectClick && this.renderRejectReasonView()}
                    {this.state.activeTabIndex === TimesheetTabIndex && !this.state.isRejectClick &&
                        <Flex column>
                            <Flex gap="gap.medium" vAlign="center">
                                <Calendar
                                    isDisabled={true}
                                    isDuplicatingEfforts={false}
                                    onCalendarEditModeChange={() => void 0}
                                    onEffortsDuplicated={() => void 0}
                                    onWeekChange={this.onCalendarWeekChange}
                                    isMobile={this.state.isMobileView}
                                    isManagerView={true}
                                    timesheetData={this.state.userTimesheet}
                                    onCalendarActiveDateChange={this.onCalendarActiveDateChange}
                                    isLoading={this.state.isLoading}
                                />
                                <Flex.Item>
                                    {this.renderCalendarDateInfo()}
                                </Flex.Item>
                            </Flex>
                            {this.renderProjectDetails()}
                        </Flex>
                    }
                    {this.renderFooter()}
                </Flex>
            </div>
        );
    }

    /** 
     * Renders the component 
     */
    render() {
        return (
            <Provider>
                <Flex>
                    <div className="request-review-container task-module-container">
                        {this.state.isMobileView ? this.renderMobileView() : this.renderDesktopView()}
                    </div>
                </Flex>
            </Provider>
        );
    }
}

export default withTranslation()(withRouter(RequestReview));