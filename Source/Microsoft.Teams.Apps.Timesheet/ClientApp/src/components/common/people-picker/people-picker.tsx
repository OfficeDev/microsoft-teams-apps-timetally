// <copyright file="people-picker.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { Dropdown, DropdownProps } from "@fluentui/react-northstar";
import { getReporteesAsync } from "../../../api/users";
import { withRouter, RouteComponentProps } from "react-router-dom";
import IUserSearchResult from "../../../models/user-search-result";

interface IDropdownProps extends RouteComponentProps {
    onUserSelectionChanged: (selectedItems: IUserDropdownItem[]) => void;
    loadingMessage: string;
    placeholder: string;
    noResultMessage: string;
    isBillable: boolean;
    existingUsers: IUserDropdownItem[];
}

export interface IUserDropdownItem {
    header: string;
    content: string;
    id: string;
    email: string;
    isBillable: boolean;
}

// Allows to search and multi-select users.
const PeoplePicker: React.FunctionComponent<IDropdownProps> = props => {
    let timeout: number | null = null; // to handle API call on user input
    let initialResults = new Array<IUserDropdownItem>();

    const [searchResult, setSearchResult] = React.useState(new Array<IUserDropdownItem>());
    const [loading, setLoading] = React.useState(true);
    const [isOpen, setOpen] = React.useState(false);

    const searchUsingAPI = async (searchQuery: string) => {
        if (initialResults.length && !searchQuery.length) {
            setSearchResult(initialResults);
            return;
        }

        let array = new Array<IUserDropdownItem>();
        const response = await getReporteesAsync(searchQuery, handleTokenAccessFailure);
        if (response.status === 200 && response.data) {
            const results: IUserSearchResult[] = response.data;
            for (let i = 0; i < results.length; i++) {
                if (results[i].displayName && results[i].userPrincipalName && props.existingUsers.filter((existingUser: IUserDropdownItem) => existingUser.id === results[i].id).length === 0) {
                    array.push({ header: results[i].displayName, content: results[i].userPrincipalName, id: results[i].id, email: results[i].userPrincipalName, isBillable: props.isBillable });
                }
            }
        }

        if (!initialResults.length) {
            initialResults = [...array];
        }

        setSearchResult(array);
        setLoading(false);
    };

    /**
    * Invoked when user changes search input
    * @param e Event details.
    */
    const initiateSearch = (e: any, { searchQuery }: DropdownProps) => {
        if (timeout) {
            window.clearTimeout(timeout);
        }
        if (!loading) {
            setLoading(true);
        }

        timeout = window.setTimeout(async () => { await searchUsingAPI(searchQuery!); }, 550);
    };

    const handleTokenAccessFailure = (error: string) => {
        props.history.push("/signin");
    };

    // Invoked when either user is added or removed
    const onUserSelectionChanged = (event: any, data: DropdownProps) => {
        props.onUserSelectionChanged(data.value as IUserDropdownItem[]);
    };

    /**
    * Invoked when user opens or closes search dropdown
    * @param e Event details.
    */
    const onOpenChange = (e: any, { open, value }: DropdownProps) => {
        if (open) {
            setLoading(true);
            searchUsingAPI("");
        }
        else {
            setSearchResult(new Array<IUserDropdownItem>());
        }
        setOpen(open!);
    };

    return (
        <div style={{ width: "100%" }}>
            <Dropdown
                search
                multiple
                styles={{ width: "100%" }}
                fluid
                loading={loading}
                loadingMessage={props.loadingMessage}
                items={searchResult}
                style={{ marginBottom: "1rem" }}
                onChange={onUserSelectionChanged}
                onSearchQueryChange={initiateSearch}
                onOpenChange={onOpenChange}
                open={isOpen}
                placeholder={props.placeholder}
                noResultsMessage={props.noResultMessage}
            />
        </div>
    );
};

export default withRouter(PeoplePicker);