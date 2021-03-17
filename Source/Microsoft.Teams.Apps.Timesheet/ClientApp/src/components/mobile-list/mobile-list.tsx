// <copyright file="mobile-list.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>
import * as React from "react";
import { ModelType } from "../../constants/constants";
import { Flex, Status, Text, Checkbox, Button, List, Divider, Avatar, Dialog } from '@fluentui/react-northstar';
import { CloseIcon, QuestionCircleIcon } from '@fluentui/react-icons-northstar';
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import Constants from "../../constants/constants";
import IProjectMemberOverview from "../../models/project-member-overview";
import IProjectTaskOverview from "../../models/project-task-overview";

import "./mobile-list.scss";

interface IMobileListProps extends WithTranslation {
    onRemove: (selectedItems: any[], modelType: ModelType) => void;
    onSelectAllItemCheckedChange: (modelType: ModelType, isAllItemsSelected: boolean) => void;
    handleCancelButtonClick: (modelType: ModelType) => void;
    onItemCheckedChange: (selectedItem: any, modelType: ModelType) => void;
    listDetails: any[];
    listOption: ModelType;
}

/**
 * Renders the projects card for the user.
 * @param props The props of type IDashboardProjectsProps.
 */
const MobileList: React.FunctionComponent<IMobileListProps> = props => {
    const localize: TFunction = props.t;
    const [isSelectMultiple, setSelectMultiple] = React.useState(false);

    React.useEffect(() => {
        getItemsCountText();
    }, [props.listDetails]);

    /**
     * Remove selected items.
     */
    const removeSelectedItems = () => {
        let items = props.listDetails;
        let itemsToRemove = items.filter((item: any) => item.isSelected);
        props.onRemove(itemsToRemove, props.listOption);
        setSelectMultiple(false);

    };

    /**
     * Handle when user checked item.
     * @param item Checked item.
     */
    const onItemCheckedChange = (selectedItem: any) => {
        props.onItemCheckedChange(selectedItem, props.listOption);
    };

    /**
     * Render member list.
     */
    const renderMemberList = () => {
        let userRequests = props.listDetails;
        if (userRequests?.length > 0) {
            let items: any[] = userRequests.map((userRequest: IProjectMemberOverview, index: number) => {
                return {
                    key: `userRequest-${index}`,
                    content:
                        <div>
                            <Flex vAlign="center" className="manage-list-item-container">
                                <Flex space="between">
                                    <Flex >
                                        <Flex vAlign="center" gap="gap.small">
                                            {userRequest.isBillable && <Status className={"billable-utilized-status"} title={localize("billable")} />}
                                            {!userRequest.isBillable && <Status className={"non-billable-utilized-status"} title={localize("nonBillable")} />}
                                            <Avatar name={userRequest.userName} />
                                            <Flex.Item>
                                                <Flex column>
                                                    <Text className="title-text" content={userRequest.userName} />
                                                    <Text className="subtitle-text" content={userRequest.totalHours} />
                                                </Flex>
                                            </Flex.Item>
                                        </Flex>
                                    </Flex>
                                </Flex>
                                <Flex.Item push>
                                    {isSelectMultiple
                                        ? <Checkbox key={index} checked={userRequest.isSelected} onChange={() => onItemCheckedChange(userRequest)} />
                                        : <Flex>
                                            <Dialog
                                                design={{ width: "30rem" }}
                                                header={<Text content={localize("removeMember", { name: userRequest.userName })} weight="semibold" />}
                                                cancelButton={localize("cancelButtonLabel")}
                                                confirmButton={localize("removeButtonLabel")}
                                                onConfirm={() => props.onRemove([userRequest], props.listOption)}
                                                trigger={
                                                    <CloseIcon outline className="close-button" />
                                                }
                                            />
                                        </Flex>}
                                </Flex.Item>
                            </Flex>
                            <Divider />
                        </div>
                };
            });
            return (
                <List className="manage-mobile-list-view" items={items} />
            );
        }
        else {
            return (getErrorMessage());
        }
    };

    /**
     * Render task list.
     */
    const renderTaskList = () => {
        let tasks = props.listDetails;
        if (tasks?.length > 0) {
            let items: any[] = tasks.map((task: IProjectTaskOverview, index: number) => {
                return {
                    key: `task-${index}`,
                    content:
                        <div>
                            <Flex className="manage-list-item-container">
                                <Flex space="between">
                                    <Flex >
                                        <Flex.Item>
                                            <Flex column>
                                                <Text className="title-text" content={task.title} />
                                                <Text className="subtitle-text" content={`${localize("addedByLabel")} ${task.title}`} />
                                            </Flex>
                                        </Flex.Item>
                                    </Flex>
                                </Flex>
                                <Flex.Item push>
                                    <Flex vAlign="center" space="between" gap="gap.small">
                                        {isSelectMultiple
                                            ? <Checkbox key={index} checked={task.isSelected} onChange={() => onItemCheckedChange(task)} />
                                            : <>
                                                <Text size="large" content={localize("hours", { hourNumber: task.totalHours })} />
                                                <Dialog
                                                    design={{ width: "30rem" }}
                                                    header={<Text content={localize("removeMember", { name: task.title })} weight="semibold" />}
                                                    cancelButton={localize("cancelButtonLabel")}
                                                    confirmButton={localize("removeButtonLabel")}
                                                    onConfirm={() => props.onRemove([task], props.listOption)}
                                                    trigger={
                                                        <CloseIcon outline className="close-button" />
                                                    }
                                                />
                                            </>}
                                    </Flex>
                                </Flex.Item>
                            </Flex>
                            <Divider />
                        </div>
                };
            });
            return (
                <List className="manage-mobile-list-view" items={items} />
            );
        }
        else {
            return (
                getErrorMessage()
            );
        }
    };

    /**
     * Return error message.
     */
    const getErrorText = () => {
        switch (props.listOption) {
            case ModelType.member:
                return localize("timesheetRequestNotAvailableHeaderDescription");
            case ModelType.task:
                return localize("taskNotAvailableHeaderDescription");
        }
    };

    /**
     * Return error component.
     */
    const getErrorMessage = () => {
        return (
            <Flex gap="gap.small" vAlign="center">
                <QuestionCircleIcon outline color="green" />
                <Text weight="bold" content={getErrorText()} />
            </Flex>);
    };

    /**
     * Renders component according to list options.
     */
    const getComponent = () => {
        switch (props.listOption) {
            case ModelType.member:
                return renderMemberList();

            case ModelType.task:
                return renderTaskList();
        }
    };

    /**
     * Gets total count of tasks.
     */
    const getItemsCountText = () => {
        switch (props.listOption) {
            case ModelType.member:
                return `${props.listDetails.length} ${localize("membersMobile")}`;

            case ModelType.task:
                return `${props.listDetails.length} ${localize("tasksMobile")}`;
        }
    };

    /** 
     * Checks if item is selected. 
     */
    const isItemSelected = () => {
        if (props.listDetails && props.listDetails.length > 0) {
            let itemCount = props.listDetails.filter((item: any) => item.isSelected)?.length;
            if (itemCount > 0) {
                return true;
            }
        }
        return false;
    };

    /**
     * Get selected count of list items.
     */
    const getSelectedItemsCountText = () => {
        if (props.listDetails.length > 0) {
            let itemCount = props.listDetails.filter((item: any) => item.isSelected)?.length;
            return `${itemCount} ${localize("selected")}`;
        }
        return "";
    };

    /**
     * Renders body for list.
     */
    const renderBody = () => {
        let body = <div>
            <Flex vAlign="center" className="list-header" padding="padding.medium">
                <Text content={getItemsCountText()} size="large" weight="semibold" />
                <Flex.Item push>
                    <div>
                        {!isSelectMultiple && props.listDetails.length > 0 && <Button className="select-multiple-button" text content={localize("selectMultiple")} onClick={() => { setSelectMultiple(!isSelectMultiple); }} />}
                        {isSelectMultiple && <Text size="medium" content={getSelectedItemsCountText()} style={{ color: "#6E6E6E" }} />}
                    </div>
                </Flex.Item>
            </Flex>
            {getComponent()}
            {isSelectMultiple && isItemSelected() &&
                <div className="footer">
                    <Flex space="between" vAlign="center">
                        <Flex.Item push >
                            <Flex gap="gap.small">
                                <Button className="remove-button" content={localize("cancelButtonLabel")} onClick={() => props.handleCancelButtonClick(props.listOption)} />
                                <Button primary className="remove-button" content={localize("removeSelected")} onClick={removeSelectedItems} />
                            </Flex>
                        </Flex.Item>
                    </Flex>
                </div>}
        </div>;
        return <div>{body}</div>;
    };

    return (
        <div className="mobile-list-container">{renderBody()}</div>
    );
};

export default withTranslation()(MobileList);