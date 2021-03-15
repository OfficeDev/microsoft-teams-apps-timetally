// <copyright file="add-members-by-billing-type.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { Icon } from 'office-ui-fabric-react';
import { Flex, Text } from "@fluentui/react-northstar";
import PeoplePicker, { IUserDropdownItem } from "../people-picker/people-picker";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";

import "./add-members-by-billing-type.scss";

interface IAddPeopleState {
    billableUsers: IUserDropdownItem[];
    nonBillableUsers: IUserDropdownItem[];
}

interface IAddPeopleProps extends WithTranslation {
    onBillableUserChanged: (billableUsers: IUserDropdownItem[]) => void;
    isMobileView: boolean;
    onNonBillableUserChanged: (nonBillableUsers: IUserDropdownItem[]) => void;
    existingUsers: IUserDropdownItem[];
}

// Wrapper to hold billable and non billable users
class AddMembersByBillingType extends React.Component<IAddPeopleProps, IAddPeopleState> {
    readonly localize: TFunction;

    // Constructor which initializes state
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.state = {
            billableUsers: [],
            nonBillableUsers: []
        };
    }

    /** Gets users which are already in project or selected in dropdown */
    getExistingUsers = () => {
        let existingUsers = this.props.existingUsers;
        existingUsers = existingUsers.concat(this.state.billableUsers);
        existingUsers = existingUsers.concat(this.state.nonBillableUsers);
        return existingUsers;
    }

    /**
    * Handler which will be invoked when user is either added or removed as billable user
    * @param selectedItem selected value of an user
    */
    onBillableUsersChanged = async (selectedItems: IUserDropdownItem[]) => {
        this.setState({ billableUsers: selectedItems });
        this.props.onBillableUserChanged(selectedItems);
    }

    /**
    * Handler which will be invoked when user is either added or removed as non-billable user
    * @param selectedItem selected value of an user
    */
    onNonBillableUsersChanged = async (selectedItems: IUserDropdownItem[]) => {
        this.setState({ nonBillableUsers: selectedItems });
        this.props.onNonBillableUserChanged(selectedItems);
    }

    /**
     * Render view for mobile.
     */
    renderMobileView = () => {
        return (
            <div className="mobile-add-people">
                <Flex gap="gap.medium" vAlign="center">
                    <Icon iconName="Contact" className="user-icon" />
                    <Flex className="people-picker">
                        <PeoplePicker
                            loadingMessage={this.localize("dropdownSearchLoadingMessage")}
                            noResultMessage={this.localize("noResultFoundDropdownMessage")}
                            placeholder={this.localize("billableEmployeePlaceholder")}
                            onUserSelectionChanged={this.onBillableUsersChanged}
                            isBillable={true}
                            existingUsers={this.getExistingUsers()}
                        />
                    </Flex>
                </Flex>
                <Flex gap="gap.medium" vAlign="center">
                    <Icon iconName="Contact" className="user-icon" />
                    <Flex className="people-picker">
                        <PeoplePicker
                            loadingMessage={this.localize("dropdownSearchLoadingMessage")}
                            noResultMessage={this.localize("noResultFoundDropdownMessage")}
                            placeholder={this.localize("nonBillableEmployeePlaceholder")}
                            onUserSelectionChanged={this.onNonBillableUsersChanged}
                            isBillable={false}
                            existingUsers={this.getExistingUsers()}
                        />
                    </Flex>
                </Flex>
            </div>);
    }

    // Renders view for web.
    renderWebView() {
        return (
            <Flex column fill>
                <Text content={this.localize("addPeopleSelectPeopleLabel")} weight="semibold" /><br />
                <Text content={this.localize("addPeopleBillableEmployeeLabel")} />
                <PeoplePicker
                    loadingMessage={this.localize("dropdownSearchLoadingMessage")}
                    noResultMessage={this.localize("noResultFoundDropdownMessage")}
                    placeholder={this.localize("billableEmployeePlaceholder")}
                    onUserSelectionChanged={this.onBillableUsersChanged}
                    isBillable={true}
                    existingUsers={this.getExistingUsers()}
                /><br />
                <Text content={this.localize("addPeopleNonBillableEmployeeLabel")} />
                <PeoplePicker
                    loadingMessage={this.localize("dropdownSearchLoadingMessage")}
                    noResultMessage={this.localize("noResultFoundDropdownMessage")}
                    placeholder={this.localize("nonBillableEmployeePlaceholder")}
                    onUserSelectionChanged={this.onNonBillableUsersChanged}
                    isBillable={false}
                    existingUsers={this.getExistingUsers()}
                />
            </Flex>);
    }

    /** 
     * Renders the component.
     */
    render() {
        if (this.props.isMobileView) {
            return this.renderMobileView();
        }
        else {
            return this.renderWebView();
        }
    }
}

export default withTranslation()(AddMembersByBillingType);