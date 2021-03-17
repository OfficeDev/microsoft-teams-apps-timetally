// <copyright file="step1.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { TFunction } from "i18next";
import { Button, Flex, Input, Text, Form } from '@fluentui/react-northstar';
import { WithTranslation, withTranslation } from "react-i18next";
import DatePickerWrapper from "../common/date-picker/date-picker";
import Constants from "../../constants/constants";
import IProject from "../../models/project";
import { cloneDeep } from "lodash";
import moment from "moment";
import * as microsoftTeams from "@microsoft/teams-js";

interface IStep1Props extends WithTranslation {
    project: IProject,
    onNextClick: (project: IProject, step: number) => void
}

interface IStep1State {
    project: IProject,
    theme: string,
    screenWidth: number,
    currentTask: string,
    isTitleValid: boolean,
    isClientNameValid: boolean,
    isStartDateValid: boolean,
    isEndDateValid: boolean,
    isBillableHoursValid: boolean,
    isNonBillableHoursValid: boolean,
    isTaskLengthValid: boolean
}

// Captures basic project details
class Step1 extends React.Component<IStep1Props, IStep1State> {
    readonly localize: TFunction;
    constructor(props: IStep1Props) {
        super(props);
        this.localize = this.props.t;

        this.state = {
            project: cloneDeep(this.props.project), // Copying from props to initialize project state which will be updated by user.
            screenWidth: window.outerWidth,
            theme: "",
            currentTask: "",
            isTitleValid: true,
            isClientNameValid: true,
            isStartDateValid: true,
            isEndDateValid: true,
            isBillableHoursValid: true,
            isNonBillableHoursValid: true,
            isTaskLengthValid: true
        }
    }

    // Adding screen resize event listener
    componentDidMount() {
        window.addEventListener("resize", this.update);
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: microsoftTeams.Context) => {
            this.setState({ theme: context.theme! });
        });
    }

    // Removing screen resize event listener
    componentWillUnmount() {
        window.removeEventListener("resize", this.update);
    }

    // Update the screen width for screen resize
    update = () => {
        this.setState({
            screenWidth: window.outerWidth
        });
    };

    /** Callback for input change of project title to update in state.
     * @param event Input event.
     * */
    onTitleChange = (event: any) => {
        let title = event.target.value;

        this.setState((prevState: IStep1State) => ({
            project: { ...prevState.project, title },
            isTitleValid: true
        }));
    }

    // Validate and return title validation error.
    getTitleValidationError = () => {
        let errorMessage = "";
        if (!this.state.isTitleValid && this.state.project.title.length === 0) {
            errorMessage = this.localize("errorRequired");
        }
        else if (!this.state.isTitleValid && this.state.project.title.length > Constants.projectTitleMaxLength) {
            errorMessage = this.localize("errorInvalidTitleLength");
        }

        return errorMessage;
    }

    /** Callback for input change of client name to update in state.
    * @param event Input event
    * */
    onClientNameChange = (event: any) => {
        let clientName = event.target.value;

        this.setState((prevState: IStep1State) => ({
            project: { ...prevState.project, clientName },
            isClientNameValid: true
        }));
    }

    /** Validate and return client name validation error.
     * @param event Input event.
     * */
    getClientNameValidationError = () => {
        let errorMessage = "";
        if (!this.state.isClientNameValid && this.state.project.clientName.length > Constants.clientNameMaxLength) {
            errorMessage = this.localize("errorInvalidClientNameLength");
        }

        return errorMessage;
    }

    /** Callback for input change of billable hours to update in state.
    * @param event Input event.
    * */
    onBillableHoursChange = (event: any) => {
        let billableHours = parseInt(event.target.value);
        this.setState((prevState: IStep1State) => ({
            project: { ...prevState.project, billableHours }
        }));
    }

    /** Callback for input change of non-billable hours to update in state.
    * @param event Input event.
    * */
    onNonBillableHoursChange = (event: any) => {
        let nonBillableHours = parseInt(event.target.value);
        this.setState((prevState: IStep1State) => ({
            project: { ...prevState.project, nonBillableHours }
        }));
    }

    /**
    * Event handler invoked on selecting start date to update in state.
    * @param date Selected date.
    */
    setStartDate = (date: Date) => {
        this.setState((prevState: IStep1State) => ({
            project: { ...prevState.project, startDate: date, endDate: date }
        }));
    }

    /**
    * Event handler invoked on selecting end date to update in state.
    *  @param endDate Selected date.
    */
    setEndDate = (endDate: Date) => {
        this.setState((prevState: IStep1State) => ({
            project: { ...prevState.project, endDate }
        }));
    }

    // Invoke when user clicks 'Next' button to add users in project.
    onNextButtonClick = (e: any) => {
        if (this.checkIfNextAllowed()) {
            this.props.onNextClick(this.state.project, 2);
        }
    }

    // Validate billable and non billable hours and return error message.
    getHoursValidationError = () => {
        if (!this.state.isBillableHoursValid && this.state.project.billableHours === 0 && this.state.project.nonBillableHours === 0) {
            return <Text error content={this.localize("errorInvalidHours")} />
        }

        return <></>;
    }

    // Function for applying validation on the fields before moving onto next step.
    checkIfNextAllowed = () => {
        let validationStatus = {
            isTitleValid: true,
            isClientNameValid: true,
            isStartDateValid: true,
            isEndDateValid: true,
            isBillableHoursValid: true,
            isNonBillableHoursValid: true,
            isTaskLengthValid: true
        };

        // Check if title is of valid length.
        if (this.state.project.title.length === 0 || this.state.project.title.length > Constants.projectTitleMaxLength) {
            validationStatus.isTitleValid = false;
        }

        // Check if client name is entered, if yes then check for valid length.
        if (this.state.project.clientName.length > Constants.clientNameMaxLength) {
            validationStatus.isClientNameValid = false;
        }

        if (!this.state.project.startDate) {
            validationStatus.isStartDateValid = false;
        }

        // Check if project end date is greater than start date.
        if (!this.state.project.endDate || moment(this.state.project.endDate).startOf("day").valueOf() < moment(this.state.project.startDate).startOf("day").valueOf()) {
            validationStatus.isEndDateValid = false;
        }

        // Hours cannot be negative number.
        if (this.state.project.billableHours < 0) {
            validationStatus.isBillableHoursValid = false;
        }

        // Hours cannot be negative number.
        if (this.state.project.nonBillableHours < 0) {
            validationStatus.isNonBillableHoursValid = false;
        }

        // Billable and non billable hours cannot be 0 at same time.
        if (this.state.project.billableHours === 0 && this.state.project.nonBillableHours === 0) {
            validationStatus.isBillableHoursValid = false;
        }

        // Validate entered task character length.
        if (this.state.currentTask.length > Constants.taskMaxLength) {
            validationStatus.isTaskLengthValid = false;
        }

        this.setState({
            isBillableHoursValid: validationStatus.isBillableHoursValid,
            isClientNameValid: validationStatus.isClientNameValid,
            isEndDateValid: validationStatus.isEndDateValid,
            isNonBillableHoursValid: validationStatus.isNonBillableHoursValid,
            isStartDateValid: validationStatus.isStartDateValid,
            isTitleValid: validationStatus.isTitleValid,
            isTaskLengthValid: validationStatus.isTaskLengthValid
        });

        return validationStatus.isBillableHoursValid &&
            validationStatus.isClientNameValid &&
            validationStatus.isEndDateValid &&
            validationStatus.isNonBillableHoursValid &&
            validationStatus.isStartDateValid &&
            validationStatus.isTitleValid &&
            validationStatus.isTaskLengthValid;
    }

    // Renders a component
    render() {
        return (
            <Flex>
                <div className="page-content">
                    <Form styles={{ width: "100%", justifyContent: "normal" }}>
                        <Form.Field
                            label={this.localize("step1ProjectTitleLabel")}
                            name="projectName"
                            id="project-name"
                            required
                            control={<Input maxLength={Constants.projectTitleMaxLength} fluid placeholder={this.localize("step1ProjectTitleInputPlaceholder")} value={this.state.project.title} onChange={this.onTitleChange} />}
                            errorMessage={this.getTitleValidationError()}
                        />
                        <Form.Field
                            label={this.localize("step1ClientNameLabel")}
                            name="clientName"
                            id="client-name"
                            control={<Input maxLength={Constants.clientNameMaxLength} fluid placeholder={this.localize("step1ClientNameInputPlaceholder")} value={this.state.project.clientName} onChange={this.onClientNameChange} />}
                            errorMessage={this.getClientNameValidationError()}
                        />

                        <Flex gap="gap.smaller">
                            <Flex.Item size="size.half">
                                <Form.Field
                                    label={this.localize("step1BillableHoursLabel")}
                                    name="billableHours"
                                    id="billable-hours"
                                    control={<Input type="number" min={0} fluid placeholder={this.localize("step1BillableHoursPlaceholder")} value={this.state.project.billableHours} onChange={this.onBillableHoursChange} />}
                                />
                            </Flex.Item>
                            <Flex.Item size="size.half">
                                <Form.Field
                                    label={this.localize("step1NonBillableHoursLabel")}
                                    name="nonBillableHours"
                                    id="non-billable-hours"
                                    control={<Input type="number" min={0} fluid placeholder={this.localize("step1NonBillableHoursPlaceholder")} value={this.state.project.nonBillableHours} onChange={this.onNonBillableHoursChange} />}
                                />
                            </Flex.Item>
                        </Flex>
                        <Flex gap="gap.smaller" className="margin-top">
                            <Flex.Item size="size.half">
                                <Form.Field
                                    label={this.localize("step1StartDateLabel")}
                                    name="startDate"
                                    id="start-date"
                                    required
                                    control={<DatePickerWrapper
                                        theme={this.state.theme}
                                        selectedDate={this.state.project.startDate!}
                                        minDate={undefined}
                                        maxDate={undefined}
                                        onDateSelect={this.setStartDate}
                                        disableSelectionForPastDate={false}
                                    />}
                                />
                            </Flex.Item>
                            <Flex.Item size="size.half">
                                <Form.Field
                                    label={this.localize("step1EndDateLabel")}
                                    name="endDate"
                                    id="end-date"
                                    required
                                    control={<DatePickerWrapper
                                        theme={this.state.theme}
                                        minDate={this.state.project.startDate}
                                        maxDate={undefined}
                                        selectedDate={this.state.project.endDate}
                                        onDateSelect={this.setEndDate}
                                        disableSelectionForPastDate={false}
                                    />}
                                />
                            </Flex.Item>
                        </Flex>
                        <Flex gap="gap.smaller" className="button-footer" vAlign="center">
                            {this.getHoursValidationError()}
                            <Flex.Item push>
                                <Form.Field
                                    control={{
                                        as: Button,
                                        content: this.localize("nextButtonLabel"),
                                        primary: true,
                                        type: "button",
                                        onClick: this.onNextButtonClick
                                    }}
                                />
                            </Flex.Item>
                        </Flex>
                    </Form>
                </div>
            </Flex>
        );
    }
}

export default withTranslation()(Step1);