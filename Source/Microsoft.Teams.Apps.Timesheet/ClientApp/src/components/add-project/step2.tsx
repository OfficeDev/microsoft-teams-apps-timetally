// <copyright file="step2.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { TFunction } from "i18next";
import { Button, Flex, ArrowLeftIcon, Divider, Text } from '@fluentui/react-northstar';
import { WithTranslation, withTranslation } from "react-i18next";
import IProject from "../../models/project";
import AddMembersByBillingType from "../common/add-members/add-members-by-billing-type";
import { IUserDropdownItem } from "../common/people-picker/people-picker";
import IProjectMember from "../../models/project-member";
import { cloneDeep } from "lodash";
import { Guid } from "guid-typescript";
import { saveProject } from "../../api/create-edit-project";
import { StatusCodes } from 'http-status-codes';
import { withRouter, RouteComponentProps } from "react-router-dom";
import ITask from "../../models/task";
import AddTask from "../common/add-task/add-task";
import moment from "moment";
import * as microsoftTeams from "@microsoft/teams-js";
import Constants from "../../constants/constants";

interface IStep2Props extends WithTranslation, RouteComponentProps {
    project: IProject,
    onBackClick: (project: IProject, step: number) => void
}

interface IStep2State {
    billableEmployees: IUserDropdownItem[],
    nonBillableEmployees: IUserDropdownItem[],
    isLoading: boolean,
    tasks: ITask[],
    errorMessage: string
}

// Step 2 for adding users in project
class Step2 extends React.Component<IStep2Props, IStep2State> {
    readonly localize: TFunction;
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;

        this.state =
        {
            billableEmployees: [],
            nonBillableEmployees: [],
            isLoading: false,
            tasks: this.props.project.tasks ? this.props.project.tasks : [],
            errorMessage: ""
        }
    }

    componentDidMount() {
        microsoftTeams.initialize();
    }

    // Invoked on submit button click for sending project details to API.
    onSubmitClick = async () => {
        this.setState({ isLoading: true, errorMessage: "" });

        let errorMessage = this.getValidationError();

        if (errorMessage) {
            this.setState({ errorMessage, isLoading: false });
            return;
        }

        let projectDetails = cloneDeep(this.props.project);
        projectDetails.members = this.mergeProjectMembers();
        projectDetails.tasks = this.state.tasks;

        var response = await saveProject(projectDetails, this.handleTokenAccessFailure);
        if (response.status === StatusCodes.CREATED) {
            microsoftTeams.tasks.submitTask({ isSuccessful: true });
        }
        else {
            this.setState({ isLoading: false });
        }
    }

    // Merged billable and non billable members in final array.
    mergeProjectMembers = () => {
        let billable: IProjectMember[] = this.state.billableEmployees.map((member: IUserDropdownItem) => { return { projectId: Guid.createEmpty().toString(), isBillable: true, userId: member.id } });
        let nonbillable: IProjectMember[] = this.state.nonBillableEmployees.map((member: IUserDropdownItem) => { return { projectId: Guid.createEmpty().toString(), isBillable: false, userId: member.id } });

        return billable.concat(nonbillable);
    }

    // Merged billable and non billable members in final array.
    getExistingUsers = () => {
        return this.state.billableEmployees.concat(this.state.nonBillableEmployees);
    }

    // Validates project details and return error if validation fails.
    getValidationError = () => {
        if (!this.state.tasks || this.state.tasks.length === 0) {
            return this.localize("addTaskAtLeastOneTaskError");
        }

        let hasTasksWithInvalidTitle = this.state.tasks.some((task: ITask) =>
            !task.title || task.title.trim().length === 0);

        if (hasTasksWithInvalidTitle) {
            return this.localize("addTaskInvalidTitle");
        }

        let isTasksHasValidDateRange = this.state.tasks.every((task: ITask) =>
            task.startDate.valueOf() >= moment(this.props.project.startDate).startOf('day').toDate().valueOf()
            && task.endDate.valueOf() <= moment(this.props.project.endDate).startOf('day').toDate().valueOf());

        if (!isTasksHasValidDateRange) {
            return this.localize("addTaskDateRangeError",
                {
                    startDate: moment(this.props.project.startDate).format("YYYY-MM-DD"),
                    endDate: moment(this.props.project.endDate).format("YYYY-MM-DD")
                });
        }

        return "";
    }

    // Invoked on previous button click to navigate back to step 1.
    onPreviousClick = () => {
        // Second parameter denotes step number.
        let previousStepProjectDetails = cloneDeep(this.props.project);
        previousStepProjectDetails.members = this.mergeProjectMembers();
        previousStepProjectDetails.tasks = cloneDeep(this.state.tasks);
        this.props.onBackClick(previousStepProjectDetails, 1);
    }

    /**
    * Invoked when billable user list is changed by either adding new or removing existing ones.
    * @param billableUsers List of updated billable users.
    */
    onBillableUserChanged = (billableUsers: IUserDropdownItem[]) => {
        this.setState({ billableEmployees: billableUsers });
    }

    /**
    * Invoked when non-billable user list is changed by either adding new or removing existing ones.
    * @param nonBillableUsers List of updated non-billable users.
    */
    onNonBillableUserChanged = (nonBillableUsers: IUserDropdownItem[]) => {
        this.setState({ nonBillableEmployees: nonBillableUsers });
    }

    /**
     * Event handler called when tasks get updated.
     * @param tasks The updated collection of tasks.
     */
    onTasksUpdated = (tasks: ITask[]) => {
        this.setState({ tasks });
    }

    handleTokenAccessFailure = (error: string) => {
        this.props.history.push("/signin");
    }

    // Renders a component
    render() {
        return (
            <>
                <Flex className="page-content" column>
                    <AddTask
                        isMobileView={window.outerWidth <= Constants.maxWidthForMobileView}
                        isAddTaskOnDoneClick={false}
                        tasks={this.state.tasks}
                        projectStartDate={this.props.project.startDate}
                        projectEndDate={this.props.project.endDate}
                        onDoneClick={() => { }}
                        onTasksUpdated={this.onTasksUpdated}
                    />
                    <Divider design={{ padding: "2rem 0" }} />
                    <AddMembersByBillingType existingUsers={this.getExistingUsers()} onBillableUserChanged={this.onBillableUserChanged} onNonBillableUserChanged={this.onNonBillableUserChanged} isMobileView={false} />
                </Flex>
                <Flex gap="gap.smaller" className="button-footer" vAlign="center">
                    <Button icon={<ArrowLeftIcon />} content={this.localize("previousButtonLabel")} onClick={this.onPreviousClick} />
                    <Flex.Item push>
                        <Flex vAlign="center" gap="gap.small">
                            {this.state.errorMessage ? <Text error content={this.state.errorMessage} /> : null}
                            <Button loading={this.state.isLoading} disabled={this.state.isLoading} content={this.localize("doneButtonLabel")} primary onClick={this.onSubmitClick} />
                        </Flex>
                    </Flex.Item>
                </Flex>
            </>
        );
    }
}

export default withTranslation()(withRouter(Step2));