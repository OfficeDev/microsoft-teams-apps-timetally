// <copyright file="project-card.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { Flex, Text } from '@fluentui/react-northstar';
import { CircularProgressbar } from "react-circular-progressbar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IDashboardProject } from "../../models/dashboard/dashboard-project";

import "react-circular-progressbar/dist/styles.css";
import "./project-card.scss";

interface IProjectCardProps extends WithTranslation {
    projectDetail: IDashboardProject;
    onClick: (projectId: string) => void;
    projectCardKey: string;
}

/**
 * Renders the project card for manager.
 * @param props The props with type IProjectCardProps.
 */
const ProjectCard: React.FunctionComponent<IProjectCardProps> = props => {
    const localize: TFunction = props.t;
    const [isUtilized, setIsUtilized] = React.useState(false);

    React.useEffect(() => {
        let totalHours = props.projectDetail.totalHours;
        let hoursUtilized = props.projectDetail.utilizedHours;
        setIsUtilized(hoursUtilized === totalHours);
    }, [props.projectDetail]);

    /**
     * Get utilization text to show as status.
     * @param projectDetail Project detail to show.
     */
    const getUtilizationText = (projectDetail: IDashboardProject) => {
        if (projectDetail) {
            if (projectDetail.utilizedHours > projectDetail.totalHours) {
                return localize("overUtilizedLabel");
            }
            else if (projectDetail.utilizedHours < projectDetail.totalHours) {
                return localize("underutilizedLabel");
            }
            else {
                return localize("fullyUtilizedLabel");
            }
        }

        return "";
    };

    /** 
     * Gets the percentage of project utilization 
     */
    const getPercentage = (projectDetail: IDashboardProject) => {
        if (projectDetail && props.projectDetail.totalHours !== 0) {
            return (props.projectDetail.utilizedHours / props.projectDetail.totalHours * 100).toFixed(2);
        }
        return 0;
    };

    /**
     * Gets project card.
     */
    const getProjectCard = () => {
        return (
            <Flex key={props.projectCardKey} className="project-card-container" vAlign="center" hAlign="center" onClick={() => props.onClick(props.projectDetail.id.toString())}>
                <Flex.Item >
                    <CircularProgressbar className="circular-progress" value={getPercentage(props.projectDetail) as number} text={`${getPercentage(props.projectDetail)}%`} />
                </Flex.Item>
                <Flex.Item push>
                    <Flex className="text-container" space="between" column fill>
                        <Text weight="semibold" className="project-title" title={props.projectDetail.title} content={props.projectDetail.title} truncated />
                        <Text size="small" className="project-subtitle" content={localize("hoursUtilizedLabel", { utilizedHours: props.projectDetail.utilizedHours, totalHours: props.projectDetail.totalHours })} />
                        <Text size="small" content={getUtilizationText(props.projectDetail)} className={isUtilized ? "fully-utilized-text" : "underutilized-text"} />
                    </Flex>
                </Flex.Item>
            </Flex>
        );
    };
    return (
        <div>{getProjectCard()}</div>
    );
};

export default withTranslation()(ProjectCard);