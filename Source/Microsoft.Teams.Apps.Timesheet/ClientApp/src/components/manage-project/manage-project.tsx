// <copyright file="manage-project.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import withContext, { IWithContext } from "../../providers/context-provider";
import { Flex, Provider, Accordion, Text, Button, Dropdown, Loader, AddIcon, EyeSlashIcon } from '@fluentui/react-northstar';
import { Icon } from 'office-ui-fabric-react';
import { ModelType } from "../../constants/constants";
import { WithTranslation, withTranslation } from "react-i18next";
import ProjectDetails from "./project-details";
import AddPeopleWrapper from "./add-people-wrapper";
import AddTask from "../common/add-task/add-task";
import MobileList from "../mobile-list/mobile-list";
import { TFunction } from "i18next";
import ProjectUtilizationTable from "../project-utilization-table/project-utilization-table";
import moment from "moment";
import IProjectMember from "../../models/project-member";
import IProjectUtilization from "../../models/project-utilization";
import IProjectMemberOverview from "../../models/project-member-overview";
import IProjectTaskOverview from "../../models/project-task-overview";
import ITask from "../../models/task";
import { IUserDropdownItem } from "../common/people-picker/people-picker";
import { Guid } from "guid-typescript";
import { getProjectUtilizationAsync, deleteMembersAsync, deleteTasksAsync, getProjectMembersOverviewAsync, getProjectTasksOverviewAsync, createTasksAsync, addMembersAsync } from "../../api/project";
import { getUserProfilesAsync } from "../../api/users";
import { withRouter, RouteComponentProps } from "react-router-dom";
import IUser from "../../models/user";
import { StatusCodes } from "http-status-codes";

import "./manage-project.scss";

interface IManageProjectState {
    members: IProjectMemberOverview[];
    projectDetails: IProjectUtilization;
    tasks: IProjectTaskOverview[];
    isLoading: boolean;
    isMemberLoading: boolean;
    isTaskLoading: boolean;
    isActionEnabled: boolean;
    dropdownMonths: string[];
    isAddPeople: boolean;
    isAddTask: boolean;
    isMobileTaskList: boolean;
    monthDropdownValue: string;
    isForbidden: boolean;
}

interface IManageProjectProps extends WithTranslation, IWithContext, RouteComponentProps {
}

// Renders task module for project utilization.
class ManageProject extends React.Component<IManageProjectProps, IManageProjectState> {
    readonly localize: TFunction;
    params: { projectId: string | undefined, isMobileView: boolean } = { projectId: undefined, isMobileView: false };

    /**
     * Constructor which initializes state. 
     */
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        let queryParams = this.props.match.params as { projectId?: string | undefined, isMobileView: string };
        this.params.projectId = queryParams.projectId;
        this.params.isMobileView = queryParams.isMobileView === "true";
        this.state = {
            members: [],
            projectDetails: {
                id: "",
                title: "",
                billableUtilizedHours: 0,
                billableUnderutilizedHours: 0,
                nonBillableUtilizedHours: 0,
                nonBillableUnderutilizedHours: 0,
                totalHours: 0,
                projectEndDate: new Date(),
                projectStartDate: new Date(),
            },
            tasks: [],
            isLoading: false,
            isMemberLoading: false,
            isTaskLoading: false,
            isActionEnabled: true,
            dropdownMonths: [],
            isAddPeople: false,
            isAddTask: false,
            isMobileTaskList: false,
            isForbidden: false,
            monthDropdownValue: "",
        };
    }

    /** 
     * Called when component get mounted.
     */
    componentDidMount() {
        this.initializeMonthYearDropdown();
        this.intializeProjectDetails();
    }

    /**
     * Gets the month's start date and end date.
     * @param month Number of month which start date and end date to get.
     */
    getMonthStartEndDate = (month: number) => {
        let date = new Date();
        let firstDay = new Date(date.getFullYear(), month, 1);
        let lastDay = new Date(date.getFullYear(), month + 1, 0);

        return [firstDay, lastDay];
    }

    /**
     * Wrapper method to get project, members and tasks details.
     */
    intializeProjectDetails = async () => {
        let currentMonth = new Date().getMonth();
        await this.getDashboardProjectUtilizationAsync(this.params.projectId!, currentMonth);
        await this.getMembersOverviewAsync(this.params.projectId!, currentMonth);
        await this.getTasksOverviewAsync(this.params.projectId!, currentMonth);
    }

    /**
     * Gets project utilization details by project Id.
     * @param projectId The project id of which details to get.
     */
    getDashboardProjectUtilizationAsync = async (projectId: string, month: number) => {
        this.setState({ isLoading: true });
        let startEndDate = this.getMonthStartEndDate(month);
        let response = await getProjectUtilizationAsync(projectId, startEndDate[0], startEndDate[1], this.handleTokenAccessFailure);
        if (response.status === StatusCodes.OK && response.data) {
            const results: IProjectUtilization = response.data;
            results.projectEndDate = new Date(results.projectEndDate);
            results.projectStartDate = new Date(results.projectStartDate);
            this.setState({ projectDetails: results, isLoading: false });
        }
        else if (response.status === StatusCodes.FORBIDDEN) {
            this.setState({ isForbidden: true, isLoading: false });
        }
        else {
            this.setState({ isLoading: false });
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
     * Convert IProjectMemberOverview to IDropdownitem.
     */
    getExistingUsers = () => {
        return this.state.members.map((member: IProjectMemberOverview) => {
            let user: IUserDropdownItem = {
                id: member.userId,
                header: member.userName,
                email: "",
                content: "",
                isBillable: member.isBillable,
            };
            return user;
        });
    }

    /**
     * Gets project members by project Id.
     * @param projectId The project id of which details to get.
     */
    getMembersOverviewAsync = async (projectId: string, month: number) => {
        this.setState({ isLoading: true });
        let startEndDate = this.getMonthStartEndDate(month);
        let response = await getProjectMembersOverviewAsync(projectId, startEndDate[0], startEndDate[1], this.handleTokenAccessFailure);
        if (response.status === StatusCodes.OK && response.data) {
            let projectMembersOverview: IProjectMemberOverview[] = response.data;
            let memberIds = projectMembersOverview.map((projectMemberOverview: IProjectMemberOverview) => projectMemberOverview.userId.toString());
            let userGraphProfiles = await this.getUsersGraphProfileAsync(memberIds);
            const members = this.mapMembersWithDisplayName(projectMembersOverview, userGraphProfiles);
            this.setState({ members, isLoading: false });
        }
        else if (response.status === StatusCodes.FORBIDDEN) {
            this.setState({ isForbidden: true, isLoading: false });
        }
        else {
            this.setState({ isLoading: false });
        }
    }

    /**
     * Gets members graph profile.
     * @param userIds Ids of members which graph profile need to fetch.
     */
    getUsersGraphProfileAsync = async (userIds: string[]) => {
        let response = await getUserProfilesAsync(userIds, this.handleTokenAccessFailure);
        if (response.status === StatusCodes.OK && response.data) {
            return response.data;
        }
        return null;
    }

    /**
     * Map members with their graph display names.
     * @param projectMembers The project members which names need to map.
     * @param userProfiles The list of user profiles.
     */
    mapMembersWithDisplayName = (projectMembers: IProjectMemberOverview[], userProfiles: IUser[]) => {
        if (userProfiles !== null) {
            // Mapping members with their graph user display name.
            for (let i = 0; i < projectMembers.length; i++) {
                let matchedUser = userProfiles.filter(user => user.id === projectMembers[i].userId);
                if (matchedUser !== null) {
                    projectMembers[i].userName = matchedUser[0].displayName;
                }
            }
        }
        return projectMembers;
    }

    /**
     * Gets project details by project Id.
     * @param projectId The project id of which details to get.
     */
    getTasksOverviewAsync = async (projectId: string, month: number) => {
        this.setState({ isLoading: true });
        let startEndDate = this.getMonthStartEndDate(month);
        let response = await getProjectTasksOverviewAsync(projectId, startEndDate[0], startEndDate[1], this.handleTokenAccessFailure);
        if (response.status === StatusCodes.OK && response.data) {
            const results: IProjectTaskOverview[] = response.data;
            this.setState({ tasks: results, isLoading: false });
        }
        else if (response.status === StatusCodes.FORBIDDEN) {
            this.setState({ isForbidden: true, isLoading: false });
        }
        else {
            this.setState({ isLoading: false });
        }
    }

    /**
     * Deep clone members state.
     */
    getCloneMembers = () => {
        return this.state.members.map((member: IProjectMemberOverview) => ({ ...member } as IProjectMemberOverview));
    }

    /**
     * Deep clone tasks state.
     */
    getCloneTasks = () => {
        return this.state.tasks.map((task: IProjectTaskOverview) => ({ ...task } as IProjectTaskOverview));
    }

    /**
     * Event handler invoked when user click on select all button.
     * @param modelType Model type of which select all button is click.
     * @param isAllItemsSelected Boolean to show if all items are selected.
     */
    onSelectAllItemCheckedChange = (modelType: ModelType, isAllItemsSelected: boolean) => {
        switch (modelType) {
            case ModelType.member:
                let members = this.getCloneMembers();
                members.map((member: IProjectMemberOverview) => {
                    member.isSelected = !isAllItemsSelected;
                });
                this.setState({ members });
                break;
            case ModelType.task:
                let tasks = this.getCloneTasks();
                tasks.map((task: IProjectTaskOverview) => {
                    task.isSelected = !isAllItemsSelected;
                });
                this.setState({ tasks });
                break;
        }
    }

    /**
     * Event handler invoked when items is checked.
     * @param selectedItem The item which is selected.
     * @param modelType Model type of the item.
     */
    onItemCheckedChange = (selectedItem: any, modelType: ModelType) => {
        switch (modelType) {
            case ModelType.member:
                let members = this.getCloneMembers();
                members.map((member: IProjectMemberOverview) => {
                    if (member.id === selectedItem.id) {
                        member.isSelected = !member.isSelected;
                    }
                });
                this.setState({ members });
                break;
            case ModelType.task:
                let tasks = this.getCloneTasks();
                tasks.map((task: IProjectTaskOverview) => {
                    if (task.id === selectedItem.id) {
                        task.isSelected = !task.isSelected;
                    }
                });
                this.setState({ tasks });
                break;
        }
    }

    /**
     * Initialize months for dropdown.
     */
    initializeMonthYearDropdown = () => {
        let months = moment.months();
        let date = new Date();
        let currentMonthIndex = date.getMonth();
        let filterMonths = months.filter((value: string, index: number) => index <= currentMonthIndex);
        let dropdownMonthYear: string[] = filterMonths.map((value: string) => (`${value} ${date.getFullYear()}`));
        let currentMonth = months.filter((value: string, index: number) => index === currentMonthIndex)[0];
        this.setState({ monthDropdownValue: `${currentMonth} ${date.getFullYear()}`, dropdownMonths: dropdownMonthYear });
    }

    /**
     * Wrapper method when user remove selected items.
     * @param selectedItems The items which are selected to remove.
     * @param modelType The model type of the item.
     */
    onRemoveSelectedItems = (selectedItems: any[], modelType: ModelType) => {
        if (selectedItems) {
            switch (modelType) {
                case ModelType.task:
                    this.onRemoveTasks(selectedItems);
                    break;
                case ModelType.member:
                    this.onRemoveMembers(selectedItems);
                    break;
            }
        }
    }

    /** 
     * Invoked when user remove task and update the task state.
     */
    onRemoveTasks = async (selectedTasks: IProjectTaskOverview[]) => {
        let tasks = this.state.tasks;
        let selectedTaskIds = selectedTasks.map((selectedTask: IProjectTaskOverview) => selectedTask.id);
        this.setState({ isTaskLoading: true });
        let response = await deleteTasksAsync(this.params.projectId!, selectedTaskIds, this.handleTokenAccessFailure);
        if (response.status === StatusCodes.NO_CONTENT) {
            selectedTasks.map((selectedTask: IProjectTaskOverview) => {
                let index = tasks.findIndex(task => task.id === selectedTask.id);
                tasks.splice(index, 1);
            });
            this.setState({ tasks, isTaskLoading: false });
        }
        else {
            this.setState({ isTaskLoading: false });
            selectedTasks.map((selectedTask: IProjectTaskOverview) => {
                selectedTask.isRemoved = false;
            });
        }
    }

    /**
     * Invoked when user remove member and update the member state.
     */
    onRemoveMembers = async (selectedMembers: IProjectMemberOverview[]) => {
        let members = this.state.members;
        selectedMembers.map((selectedMember: IProjectMemberOverview) => {
            selectedMember.isRemoved = true;
        });
        this.setState({ isMemberLoading: true });
        let response = await deleteMembersAsync(this.params.projectId!, selectedMembers, this.handleTokenAccessFailure);
        if (response.status === StatusCodes.NO_CONTENT) {
            selectedMembers.map((selectedMember: IProjectMemberOverview) => {
                let index = members.findIndex(member => member.id === selectedMember.id);
                members.splice(index, 1);
            });
            this.setState({ members, isMemberLoading: false });
        }
        else {
            this.setState({ isMemberLoading: false });
            selectedMembers.map((selectedMember: IProjectMemberOverview) => {
                selectedMember.isRemoved = false;
            });
        }
    }

    /**
     * Adds member in database.
     * @param selectedUsers Selected users to add.
     */
    AddMembersAsync = async (selectedUsers: IProjectMember[]) => {
        if (selectedUsers.length > 0) {
            let currentMonth = new Date().getMonth();
            this.setState({ isMemberLoading: true });
            let response = await addMembersAsync(this.params.projectId!, selectedUsers, this.handleTokenAccessFailure);
            if (response.status === StatusCodes.OK) {
                await this.getMembersOverviewAsync(this.params.projectId!, currentMonth);
            }
        }
        this.setState({ isMemberLoading: false });
        this.onAddPeopleButtonClick();
    }

    /**
     * Invoked when user click Add button.
     */
    onAddPeopleButtonClick = () => {
        this.setState((prevState: IManageProjectState) => ({
            isAddPeople: !prevState.isAddPeople
        }));
    }

    /**
     * Invoked when user click Add Task button.
     */
    onAddTaskButtonClick = () => {
        this.setState((prevState: IManageProjectState) => ({
            isAddTask: !prevState.isAddTask
        }));
    }

    /**
     * Renders add people component.
     */
    renderAddPeople = () => {
        let existingUsers = this.getExistingUsers();
        return <AddPeopleWrapper
            existingUsers={existingUsers}
            isMobileView={this.params.isMobileView}
            onDoneClick={this.AddMembersAsync}
            projectId={this.state.projectDetails.id}
            isLoading={this.state.isMemberLoading}
        />;
    }

    /**
     * Renders add task component.
     */
    renderAddTask = () => {
        return <AddTask
            isAddTaskOnDoneClick={true}
            onDoneClick={this.onTaskAdded}
            isMobileView={this.params.isMobileView}
            isLoading={this.state.isTaskLoading}
            projectStartDate={this.state.projectDetails.projectStartDate}
            projectEndDate={this.state.projectDetails.projectEndDate}
        />;
    }

    /**
     * Handled month year dropdown value change.
     * @param event Keyboard/mouse event.
     * @param data Selected dropdown value.
     */
    handleMonthYearDropdownChange = async (event: any, data: any) => {
        let months = moment.months();
        let selectedMonth = months.findIndex((value: string) => value === data.value.split(" ")[0].trim());
        await this.getDashboardProjectUtilizationAsync(this.params.projectId!, selectedMonth);
        await this.getMembersOverviewAsync(this.params.projectId!, selectedMonth);
        await this.getTasksOverviewAsync(this.params.projectId!, selectedMonth);
        this.setState({ monthDropdownValue: data.value });
    }

    /**
     * Event handler when user click cancel button.
     * @param modelType Model type of the cancel button is clicked.
     */
    handleCancelButtonClick = (modelType: ModelType) => {
        switch (modelType) {
            case ModelType.member:
                let members = this.getCloneMembers();
                members.map((member: IProjectMemberOverview) => {
                    member.isSelected = false;
                });
                this.setState({ members });
                break;
            case ModelType.task:
                let tasks = this.getCloneTasks();
                tasks.map((task: IProjectTaskOverview) => {
                    task.isSelected = false;
                });
                this.setState({ tasks });
                break;
        }
    }

    /** 
     * Handle when user switch between tasks and members.
     */
    onScreenToggle = () => {
        this.setState((prevState: IManageProjectState) => ({
            isMobileTaskList: !prevState.isMobileTaskList
        }));
    }


    /**
     * Invoked when user add tasks.
     * @param tasks Tasks needs to be added.
     */
    onTaskAdded = async (tasks: ITask[]) => {
        if (tasks.length > 0) {
            let currentMonth = new Date().getMonth();
            let tasksOverview: IProjectTaskOverview[] = [];
            tasks.map((task: ITask) => {
                let taskOverview: IProjectTaskOverview = {
                    id: Guid.createEmpty().toString(),
                    title: task.title,
                    projectId: this.params.projectId!,
                    totalHours: 0,
                    isRemoved: false,
                    isSelected: false,
                    startDate: task.startDate,
                    endDate: task.endDate,
                };
                tasksOverview.push(taskOverview);
            });
            this.setState({ isTaskLoading: true });
            let response = await createTasksAsync(this.params.projectId!, tasksOverview, this.handleTokenAccessFailure);
            if (response.status === StatusCodes.CREATED) {
                await this.getTasksOverviewAsync(this.params.projectId!, currentMonth);
            }
        }

        this.setState((prevState: IManageProjectState) => ({
            isTaskLoading: false,
            isLoading: false,
            isAddTask: !prevState.isAddTask
        }));
    }

    /** 
     * Renders body.
     */
    renderBody = () => {
        if (!this.params.isMobileView) {
            return (
                <div>
                    <Flex hAlign="center" vAlign="center" >
                        <Flex.Item>
                            <Text content={this.state.projectDetails.title} size="large" weight="semibold" />
                        </Flex.Item>
                        <Flex.Item push>
                            <Flex className="date-dropdown">
                                <Dropdown
                                    items={this.state.dropdownMonths}
                                    fluid
                                    className="date-dropdown"
                                    onChange={this.handleMonthYearDropdownChange}
                                    value={this.state.monthDropdownValue}
                                />
                            </Flex>
                        </Flex.Item>
                    </Flex>
                    <Flex vAlign="center" >
                        <Accordion
                            style={{ width: "100%" }}
                            panels={
                                [
                                    {
                                        title: this.localize("utilization"),
                                        content: <ProjectDetails projectDetail={this.state.projectDetails} isMobile={false} theme={this.props.teamsContext?.theme!} />
                                    },
                                    {
                                        title: `${this.localize("members")} (${this.state.members.length})`,
                                        content:
                                            <ProjectUtilizationTable
                                                onAddActionClick={this.onAddPeopleButtonClick}
                                                onSelectAllItemCheckedChange={this.onSelectAllItemCheckedChange}
                                                onItemCheckedChange={this.onItemCheckedChange}
                                                onRemove={this.onRemoveMembers}
                                                tableDetails={this.state.members}
                                                tableOptions={ModelType.member}
                                                isLoading={this.state.isMemberLoading}
                                            />
                                    },
                                    {
                                        title: `${this.localize("tasks")} (${this.state.tasks.length})`,
                                        content:
                                            <ProjectUtilizationTable
                                                onAddActionClick={this.onAddTaskButtonClick}
                                                onSelectAllItemCheckedChange={this.onSelectAllItemCheckedChange}
                                                onItemCheckedChange={this.onItemCheckedChange}
                                                onRemove={this.onRemoveSelectedItems}
                                                tableDetails={this.state.tasks}
                                                tableOptions={ModelType.task}
                                                isLoading={this.state.isTaskLoading}
                                            />
                                    },
                                ]}
                            exclusive
                        />
                    </Flex>
                </div>
            );
        }
        else {
            return (
                <div>
                    <Flex className="mobile-action-toolkit">
                        <Flex.Item push>
                            <Flex gap="gap.medium" vAlign="center">
                                <AddIcon className="mobile-action-button" onClick={this.onAddTaskButtonClick} />
                                <Icon className="mobile-action-button" onClick={this.onAddPeopleButtonClick} iconName="AddFriend" />
                            </Flex>
                        </Flex.Item>
                    </Flex>
                    <Flex hAlign="center" className="date-dropdown">
                        <Dropdown
                            items={this.state.dropdownMonths}
                            fluid
                            className="date-dropdown"
                            onChange={this.handleMonthYearDropdownChange}
                            value={this.state.monthDropdownValue}
                        />
                    </Flex>
                    <Flex vAlign="center">
                        <Text content={this.state.projectDetails.title} size="medium" weight="semibold" />
                        <Flex.Item push>
                            <Button text primary content={this.state.isMobileTaskList ? this.localize("viewMemberButtonLabel") : this.localize("viewTaskButtonLabel")} onClick={this.onScreenToggle} />
                        </Flex.Item>
                    </Flex>
                    <Flex vAlign="center">
                        <ProjectDetails projectDetail={this.state.projectDetails} isMobile={true} theme={this.props.teamsContext?.theme!} />

                    </Flex>
                    {this.state.isMobileTaskList &&
                        <MobileList
                            onSelectAllItemCheckedChange={this.onSelectAllItemCheckedChange}
                            handleCancelButtonClick={this.handleCancelButtonClick}
                            onItemCheckedChange={this.onItemCheckedChange}
                            onRemove={this.onRemoveSelectedItems}
                            listDetails={this.state.tasks}
                            listOption={ModelType.task} />}
                    {!this.state.isMobileTaskList &&
                        <MobileList
                            onSelectAllItemCheckedChange={this.onSelectAllItemCheckedChange}
                            handleCancelButtonClick={this.handleCancelButtonClick}
                            onItemCheckedChange={this.onItemCheckedChange}
                            onRemove={this.onRemoveSelectedItems}
                            listDetails={this.state.members}
                            listOption={ModelType.member} />}
                </div>
            );
        }
    }

    // Renders the component
    render() {
        if (this.state.isLoading) {
            return <Loader />;
        }
        if (this.state.isForbidden) {
            return (
                <Flex column hAlign="center" vAlign="center" design={{ height: "50vh" }}>
                    <EyeSlashIcon size="medium" />
                    <Text content={this.localize("manageProjectNotAccessibleMessage")} size="medium" />
                </Flex>
            );
        }

        return (
            <Provider>
                <Flex>
                    <div className="manage-project-container">
                        {!this.state.isAddPeople && !this.state.isAddTask && this.renderBody()}
                        {this.state.isAddPeople && !this.state.isAddTask && this.renderAddPeople()}
                        {!this.state.isAddPeople && this.state.isAddTask && this.renderAddTask()}
                    </div>
                </Flex>
            </Provider>
        );
    }
}

export default withTranslation()(withContext(withRouter(ManageProject)));