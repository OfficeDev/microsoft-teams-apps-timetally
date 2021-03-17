// <copyright file="dashboard-projects-wrapper.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { Flex, Text } from "@fluentui/react-northstar";
import { QuestionCircleIcon } from '@fluentui/react-icons-northstar';
import ProjectCard from "../../components/project-card/project-card";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IDashboardProject } from "../../models/dashboard/dashboard-project";

interface IDashboardProjectsProps extends WithTranslation {
    onProjectCardClick: (projectId: string) => void;
    projects: IDashboardProject[];
    isMobileView: boolean;
    searchText: string;
}

/**
 * Renders the project cards for the user.
 * @param props The props of type IDashboardProjectsProps.
 */
const DashboardProjectsWrapper: React.FunctionComponent<IDashboardProjectsProps> = props => {
    const localize: TFunction = props.t;

    /** 
     * Renders project card for each project.
     */
    const renderProjects = () => {
        if (!props.projects || props.projects.length === 0) {
            return <Flex className="manage-timesheet-request-content" gap="gap.small">
                <Flex.Item>
                    <div className="error-container">
                        <QuestionCircleIcon outline />
                    </div>
                </Flex.Item>
                <Flex.Item grow>
                    <Flex column gap="gap.small" vAlign="stretch">
                        <div>
                            <Text weight="bold" content={localize("noProjectsAvailable")} /><br />
                            {props.searchText !== "" &&
                                <Text
                                    content={localize("timesheetProjectsNotFoundForSearchedTextDescription", { searchedText: props.searchText })}
                                />
                            }
                        </div>
                    </Flex>
                </Flex.Item>
            </Flex>;
        }

        let projects = props.projects.map((project: IDashboardProject, index: number) => {
            return <ProjectCard key={`project-parent-${index}`} projectCardKey={`project-${index}`} projectDetail={project} onClick={props.onProjectCardClick} />;
        });

        return <Flex vAlign="center" hAlign={props.isMobileView ? undefined : "center"} >{projects}</Flex>;
    };

    return renderProjects();
};

export default withTranslation()(DashboardProjectsWrapper);