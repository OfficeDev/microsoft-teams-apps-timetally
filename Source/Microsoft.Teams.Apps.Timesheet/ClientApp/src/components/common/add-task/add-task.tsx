// <copyright file="add-task.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { Flex, Text, Input, Button, Loader } from "@fluentui/react-northstar";
import { AddIcon, CloseIcon } from '@fluentui/react-icons-northstar';
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import DatePickerWrapper from "../../common/date-picker/date-picker";
import ITask from "../../../models/task";
import { cloneDeep } from "lodash";
import { Guid } from "guid-typescript";
import moment from "moment";
import * as microsoftTeams from "@microsoft/teams-js";

import "./add-task.scss";

interface IAddTaskProps extends WithTranslation {
    tasks?: ITask[];
    isMobileView: boolean;
    projectStartDate: Date;
    projectEndDate: Date;
    isAddTaskOnDoneClick: boolean;
    onTasksUpdated?: (tasks: ITask[]) => void;
    onDoneClick: (filteredTasks: ITask[]) => void;
    isLoading?: boolean;
}

interface IAddTaskState {
    tasks: ITask[];
    mobileInput: string;
    key: number;
    theme: string;
}

// Render component to add task in project
class AddTask extends React.Component<IAddTaskProps, IAddTaskState> {
    readonly localize: TFunction;

    // Constructor which initializes state
    constructor(props: IAddTaskProps) {
        super(props);
        this.localize = this.props.t;

        this.state = {
            tasks: this.props.tasks ? cloneDeep(this.props.tasks) : [],
            mobileInput: "",
            key: 0,
            theme: ""
        };
    }

    // Adding screen resize event listener
    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: microsoftTeams.Context) => {
            this.setState({ theme: context.theme! });
        });
    }

    // Handler which will be invoked when user clicked "+ Add row" button.
    onTaskRowAdded = async () => {
        let tasks = this.state.tasks ? cloneDeep(this.state.tasks) : [];
        tasks.push({
            id: Guid.createEmpty().toString(),
            title: "",
            startDate: moment(this.props.projectStartDate).startOf('day').toDate(),
            endDate: moment(this.props.projectEndDate).startOf('day').toDate(),
            projectId: Guid.createEmpty().toString()
        });

        if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
            this.props.onTasksUpdated(tasks);
        }

        this.setState({ tasks });
    }

    /**
     * Event handler called when a task title get changed.
     * @param taskAtIndex The index in array where task details available.
     * @param value The update task title.
     */
    onTaskInputChange = async (taskAtIndex: number, value: any) => {
        let tasks: ITask[] = cloneDeep(this.state.tasks);

        if (taskAtIndex > -1 && taskAtIndex < tasks.length) {
            let taskToUpdate = tasks[taskAtIndex];

            if (taskToUpdate) {
                taskToUpdate.title = value;

                if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
                    this.props.onTasksUpdated(tasks);
                }

                this.setState({ tasks });
            }
        }
    }

    /**
     * Handle when user clicks on done button.
     */
    handleDoneButtonClick = () => {
        let tasks = this.state.tasks;
        let filteredTasks: ITask[] = [];

        tasks.map((task: ITask) => {
            if (task.title?.trim().length > 0) {
                filteredTasks.push(task);
            }
        });
        this.props.onDoneClick(filteredTasks);
    }

    /**
     * Invoked from mobile when user enter tasks.
     * @param value Input tasks
     */
    onMobileInputChange = (value: string) => {
        let inputTasks = value.split(",");
        let tasks: any = [];
        inputTasks.map((inputTask: any) => {
            tasks.push(inputTask.trim());
        });
        this.setState((prevState: IAddTaskState) => ({
            tasks,
            mobileInput: value,
            key: prevState.key + 1
        }));
    }

    /**
     * Event handler invoked on selecting start date of a task.
     * @param taskAtIndex The index in array of which task details needs to be updated.
     * @param date The selected date.
     */
    onStartDateChange = (taskAtIndex: number, date: Date) => {
        let tasks: ITask[] = cloneDeep(this.state.tasks);

        if (taskAtIndex > -1 && taskAtIndex < tasks.length) {
            let taskToUpdate = tasks[taskAtIndex];

            if (taskToUpdate) {
                taskToUpdate.startDate = moment(date).startOf('day').toDate();

                if (taskToUpdate.startDate > taskToUpdate.endDate) {
                    taskToUpdate.endDate = moment(date).startOf('day').toDate();
                }

                if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
                    this.props.onTasksUpdated(tasks);
                }

                this.setState({ tasks });
            }
        }
    }

    /**
     * Event handler invoked on selecting end date of a task.
     * @param taskAtIndex The index in array of which task details needs to be updated.
     * @param date The selected date.
     */
    onEndDateChange = (taskAtIndex: number, date: Date) => {
        let tasks: ITask[] = cloneDeep(this.state.tasks);

        if (taskAtIndex > -1 && taskAtIndex < tasks.length) {
            let taskToUpdate = tasks[taskAtIndex];

            if (taskToUpdate) {
                taskToUpdate.endDate = moment(date).startOf('day').toDate();

                if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
                    this.props.onTasksUpdated(tasks);
                }

                this.setState({ tasks });
            }
        }
    }

    /**
     * Event handler called when deleting a task.
     * @param taskAtIndex The index in array of which task to be deleted.
     */
    onDeleteTask = (taskAtIndex: number) => {
        let tasks: ITask[] = cloneDeep(this.state.tasks);

        if (taskAtIndex > -1 && taskAtIndex < tasks.length) {
            tasks.splice(taskAtIndex, 1);

            if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
                this.props.onTasksUpdated(tasks);
            }

            this.setState({ tasks });
        }
    }

    /**
     * Render task input row
     */
    renderTaskInputRow = () => {
        let counter = 0;
        return (<Flex gap="gap.small" vAlign="center" column>
            {this.state.tasks.map((task: ITask, index: number) => {
                return <Flex key={`project-task-${index}`} gap="gap.medium" vAlign="center">
                    <Text size="small" content={`${++counter}.`} design={{ marginTop: index === 0 ? "2.5rem" : "0" }} />
                    {
                        index === 0 ?
                            <Flex column gap="gap.small" fill>
                                <Text size="small" content={this.localize("taskNameLabel")} />
                                <Input
                                    className="input"
                                    type="text"
                                    placeholder={this.localize('taskNameInputPlaceholder')}
                                    onChange={(event: any) => this.onTaskInputChange(index, event.target.value)}
                                    value={task.title}
                                    title={task.title}
                                    fluid
                                    data-tid={`task-title-${index}`}
                                />
                            </Flex> :
                            <Input
                                className="input"
                                type="text"
                                placeholder={this.localize('taskNameInputPlaceholder')}
                                onChange={(event: any) => this.onTaskInputChange(index, event.target.value)}
                                value={task.title}
                                title={task.title}
                                fluid
                                data-tid={`task-title-${index}`}
                            />
                    }
                    {
                        index === 0 ?
                            <Flex column gap="gap.small">
                                <Text size="small" content={this.localize("addTaskStartDate")} />
                                <DatePickerWrapper
                                    className="add-task-datepicker"
                                    theme={this.state.theme}
                                    selectedDate={task.startDate}
                                    maxDate={this.props.projectEndDate}
                                    minDate={this.props.projectStartDate}
                                    onDateSelect={(date: Date) => this.onStartDateChange(index, date)}
                                    disableSelectionForPastDate={false}
                                />
                            </Flex> :
                            <DatePickerWrapper
                                className="add-task-datepicker"
                                theme={this.state.theme}
                                selectedDate={task.startDate}
                                minDate={this.props.projectStartDate}
                                maxDate={this.props.projectEndDate}
                                onDateSelect={(date: Date) => this.onStartDateChange(index, date)}
                                disableSelectionForPastDate={false}
                            />
                    }
                    {
                        index === 0 ?
                            <Flex column gap="gap.small">
                                <Text size="small" content={this.localize("addTaskEndDate")} />
                                <DatePickerWrapper
                                    className="add-task-datepicker"
                                    theme={this.state.theme}
                                    selectedDate={task.endDate}
                                    minDate={this.state.tasks[index].startDate}
                                    maxDate={this.props.projectEndDate}
                                    onDateSelect={(date: Date) => this.onEndDateChange(index, date)}
                                    disableSelectionForPastDate={false}
                                />
                            </Flex> :
                            <DatePickerWrapper
                                className="add-task-datepicker"
                                theme={this.state.theme}
                                selectedDate={task.endDate}
                                minDate={this.state.tasks[index].startDate}
                                maxDate={this.props.projectEndDate}
                                onDateSelect={(date: Date) => this.onEndDateChange(index, date)}
                                disableSelectionForPastDate={false}
                            />
                    }
                    <CloseIcon
                        className="cursor-pointer"
                        design={{ marginTop: index === 0 ? "2.5rem" : "0" }}
                        onClick={() => { this.onDeleteTask(index); }} />
                </Flex>;
            })}
            <Button className="add-row-button" data-tid="addRowButton" icon={<AddIcon outline />} content={this.localize("addRowButtonLabel")} onClick={this.onTaskRowAdded} />
        </Flex>);
    }

    // Render view for mobile
    renderMobileInput = () => {
        let counter = 0;
        return (<Flex gap="gap.small" vAlign="center" column>
            {this.state.tasks.map((task: ITask, index: number) => {
                return <Flex key={`project-task-${index}`} gap="gap.medium">
                    <Text size="small" content={`${++counter}.`} design={{ marginTop: index === 0 ? "2.5rem" : "0" }} />
                    {
                        <Flex column gap="gap.small">
                            <Flex gap="gap.small" vAlign="center">
                                <Flex column gap="gap.smaller" fill>
                                    <Text size="small" content={this.localize("taskNameLabel")} />
                                    <Input
                                        className="input"
                                        type="text"
                                        placeholder={this.localize('taskNameInputPlaceholder')}
                                        onChange={(event: any) => this.onTaskInputChange(index, event.target.value)}
                                        value={task.title}
                                        title={task.title}
                                        fluid
                                    />
                                </Flex>
                                <CloseIcon
                                    className="cursor-pointer"
                                    design={{ marginTop: index === 0 ? "2.5rem" : "0" }}
                                    onClick={() => { this.onDeleteTask(index); }} />
                            </Flex>
                            <Flex gap="gap.small">
                                <Flex column gap="gap.smaller">
                                    <Text size="small" content={this.localize("addTaskStartDate")} />
                                    <DatePickerWrapper
                                        className="add-task-datepicker"
                                        theme={this.state.theme}
                                        selectedDate={task.startDate}
                                        minDate={this.props.projectStartDate}
                                        maxDate={this.props.projectEndDate}
                                        onDateSelect={(date: Date) => this.onStartDateChange(index, date)}
                                        disableSelectionForPastDate={false}
                                    />
                                </Flex>
                                <Flex column gap="gap.smaller">
                                    <Text size="small" content={this.localize("addTaskEndDate")} />
                                    <DatePickerWrapper
                                        className="add-task-datepicker"
                                        theme={this.state.theme}
                                        selectedDate={task.endDate}
                                        minDate={this.state.tasks[index].startDate}
                                        maxDate={this.props.projectEndDate}
                                        onDateSelect={(date: Date) => this.onEndDateChange(index, date)}
                                        disableSelectionForPastDate={false}
                                    />
                                </Flex>
                            </Flex>
                        </Flex>
                    }
                </Flex>
            })}
            <Button styles={{ marginTop: "0.5rem" }} fluid size="small" icon={<AddIcon outline />} content={this.localize("addRowButtonLabel")} onClick={this.onTaskRowAdded} />
        </Flex>);
    }

    // Renders the component
    render() {
        if (this.props.isLoading && this.props.isLoading) {
            return <Loader />;
        }

        return (
            <div className="add-task-container">
                <Flex column fill >
                    <Text content={this.localize("addTaskTaskModuleHeader")} weight="semibold" /><br />
                    <div className={this.props.isMobileView ? "input-rows-mobile" : "input-rows-desktop"}>
                        {!this.props.isMobileView && this.renderTaskInputRow()}
                        {this.props.isMobileView && this.renderMobileInput()}
                    </div>
                </Flex>
                { this.props.isAddTaskOnDoneClick ?
                    <div className="footer">
                        <Flex>
                            <Flex.Item push>
                                <Button primary className="action-button" data-tid="submitTasks" content={this.localize("doneButtonLabel")} onClick={this.handleDoneButtonClick} />
                            </Flex.Item>
                        </Flex>
                    </div> : null}
            </div>);

    }
}

export default withTranslation()(AddTask);