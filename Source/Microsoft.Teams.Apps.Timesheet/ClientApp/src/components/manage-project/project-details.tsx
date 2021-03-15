// <copyright file="project-details.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { Flex, Text, Status } from '@fluentui/react-northstar';
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import Constants, { Themes } from "../../constants/constants";
import IProjectUtilization from "../../models/project-utilization";
import Donut from "react-donut";

import "react-circular-progressbar/dist/styles.css";
import "./manage-project.scss";

interface IProjectDetailsProps extends WithTranslation {
    projectDetail: IProjectUtilization;
    isMobile: boolean;
    theme: string;
}

/**
 * Renders the project details and donut chart.
 * @param props The props with type IProjectDetailsProps.
 */
const ProjectDetails: React.FunctionComponent<IProjectDetailsProps> = props => {
    const localize: TFunction = props.t;
    const isUtilized: boolean = props.projectDetail.totalHours === props.projectDetail.billableUtilizedHours + props.projectDetail.nonBillableUtilizedHours;

    /**
     * Get utilization status.
     */
    const getUtilizationStatus = () => {
        let total = props.projectDetail.totalHours;
        let totalUtilized = props.projectDetail.billableUtilizedHours + props.projectDetail.nonBillableUtilizedHours;

        if (totalUtilized > total && !isUtilized) {
            return localize("overUtilizedLabel");
        }
        else if (totalUtilized < total) {
            return localize("underutilizedLabel");
        }
        else if (totalUtilized === total) {
            return localize("fullyUtilizedLabel");
        }
    };

    /**
     * Get view for web.
     */
    const getWebView = () => {
        return <Flex gap="gap.small">
            <Flex className="text-container" space="between" column>
                <Flex vAlign="center" gap="gap.medium" >
                    <Status className="status-bullets billable-utilized-status" title={localize("billableUtilized")} />
                    {props.isMobile && <Text className={"status-label-mobile"} size="medium" content={localize("billableUtilized")} />}
                    {!props.isMobile && <Text className={"status-label-web"} size="medium" content={localize("billableUtilized")} />}
                </Flex>
                <Flex vAlign="center" gap="gap.medium">
                    <Status className="status-bullets billable-underutilized-status" title={localize("billableUnutilized")} />
                    {props.isMobile && <Text className={"status-label-mobile"} content={localize("billableUnutilized")} />}
                    {!props.isMobile && <Text className={"status-label-web"} content={localize("billableUnutilized")} />}
                </Flex>
                <Flex vAlign="center" gap="gap.medium">
                    <Status className="status-bullets non-billable-utilized-status" title={localize("nonBillableUtilized")} />
                    {props.isMobile && <Text className={"status-label-mobile"} content={localize("nonBillableUtilized")} />}
                    {!props.isMobile && <Text className={"status-label-web"} content={localize("nonBillableUtilized")} />}
                </Flex>
                <Flex vAlign="center" gap="gap.medium">
                    <Status className="status-bullets non-billable-underutilized-status" title={localize("nonBillableUnutilized")} />
                    {props.isMobile && <Text className={"status-label-mobile"} content={localize("nonBillableUnutilized")} />}
                    {!props.isMobile && <Text className={"status-label-web"} content={localize("nonBillableUnutilized")} />}
                </Flex>
            </Flex>
            <Flex className="text-container" space="between" column>
                <Text truncated className={props.isMobile ? "status-label-mobile" : "status-label-web"} size="medium" content={localize("hours", { hourNumber: props.projectDetail.billableUtilizedHours })} />
                <Text truncated className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("hours", { hourNumber: props.projectDetail.billableUnderutilizedHours })} />
                <Text truncated className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("hours", { hourNumber: props.projectDetail.nonBillableUtilizedHours })} />
                <Text truncated className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("hours", { hourNumber: props.projectDetail.nonBillableUnderutilizedHours })} />
            </Flex>
        </Flex>;
    };

    /**
     * Get view to show for mobile.
     */
    const getMobileView = () => {
        return <Flex gap="gap.small">
            <Flex className="text-container mobile-text" space="between" column>
                <Flex vAlign="center" >
                    <Flex vAlign="center" gap="gap.medium">
                        <Status className="status-bullets billable-utilized-status" title={localize("billableUtilized")} />
                        <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} size="medium" content={`${localize("billableUtilized")}`} />
                    </Flex>
                    <Flex.Item push>
                        <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} size="medium" content={localize("hours", { hourNumber: props.projectDetail.billableUtilizedHours })} />
                    </Flex.Item>
                </Flex>
                <Flex vAlign="center">
                    <Flex vAlign="center" gap="gap.medium">
                        <Status className="status-bullets billable-underutilized-status" title={localize("billableUnutilized")} />
                        <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={`${localize("billableUnutilized")}`} />
                    </Flex>
                    <Flex.Item push>
                        <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("hours", { hourNumber: props.projectDetail.billableUnderutilizedHours })} />
                    </Flex.Item>
                </Flex>
                <Flex vAlign="center">
                    <Flex vAlign="center" gap="gap.medium">
                        <Status className="status-bullets non-billable-utilized-status" title={localize("nonBillableUtilized")} />
                        <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={`${localize("nonBillableUtilized")}`} />
                    </Flex>
                    <Flex.Item push>
                        <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("hours", { hourNumber: props.projectDetail.nonBillableUtilizedHours })} />
                    </Flex.Item>
                </Flex>
                <Flex vAlign="center">
                    <Flex vAlign="center" gap="gap.medium">
                        <Status className="status-bullets non-billable-underutilized-status" title={localize("nonBillableUnutilized")} />
                        <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={`${localize("nonBillableUnutilized")}`} />
                    </Flex>
                    <Flex.Item push>
                        <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("hours", { hourNumber: props.projectDetail.nonBillableUnderutilizedHours })} />
                    </Flex.Item>
                </Flex>
            </Flex>
        </Flex>;
    };

    /**
     * Get donut colors
     */
    const getDonutColors = () => {
        if (props.theme === Themes.contrast) {
            return [Constants.billableContrastStatusColor, Constants.underutilizedBillableContrastStatusColor, Constants.nonBillableContrastStatusColor, Constants.underutilizedNonBillableContrastStatusColor];
        }
        return [Constants.billableStatusColor, Constants.underutilizedBillableStatusColor, Constants.nonBillableStatusColor, Constants.underutilizedNonBillableStatusColor];
    }

    /**
     * Get project details.
     */
    const getProjectDetail = () => {
        let isOverutilized = props.projectDetail.totalHours < props.projectDetail.billableUtilizedHours + props.projectDetail.nonBillableUtilizedHours;
        let isNonBillableOverutilized = props.projectDetail.nonBillableUnderutilizedHours < 0;
        let isBillableOverutilized = props.projectDetail.billableUnderutilizedHours < 0;

        return <div >
            <Flex vAlign="center">
                <Flex.Item >
                    <div className={!props.isMobile ? "donut-container" : "donut-container-mobile"}>
                        {isOverutilized || isBillableOverutilized || isNonBillableOverutilized ?
                            <Flex hAlign="center" vAlign="center" className="total-target-label" column>
                                <Text weight="semibold" size="medium" content={getUtilizationStatus()} className={isUtilized ? "project-meets-target" : "project-below-target"} />
                            </Flex>
                            :
                            <Donut
                                chartData={[
                                    { name: ` `, data: props.projectDetail.billableUtilizedHours },
                                    { name: ` `, data: props.projectDetail.billableUnderutilizedHours },
                                    { name: ` `, data: props.projectDetail.nonBillableUtilizedHours },
                                    { name: ` `, data: props.projectDetail.nonBillableUnderutilizedHours },
                                ]}
                                chartThemeConfig={{
                                    chart: {
                                        background: "transparent",
                                    },
                                    series: {
                                        colors: getDonutColors(),
                                    },
                                    chartExportMenu: {
                                        backgroundColor: "transparent",
                                        color: "transparent"
                                    }
                                }}
                                showChartLabel={false}
                                chartRadiusRange={[`90%`, `100%`]}
                                chartWidth={!props.isMobile ? 250 : 200}
                                chartHeight={!props.isMobile ? 200 : 170}
                                title=""
                                legendAlignment={"top"}
                            />}
                        <Flex vAlign="center" hAlign="center" column>
                            {!isOverutilized && !isBillableOverutilized && !isNonBillableOverutilized && <Text weight="semibold" size="medium" content={getUtilizationStatus()} className={isUtilized ? "project-meets-target" : "project-below-target"} />}
                        </Flex>
                    </div>
                </Flex.Item>
                <Flex.Item >
                    {props.isMobile ? getMobileView() : getWebView()}
                </Flex.Item>
            </Flex>
        </div>;
    };
    return (
        <div className="project-details-container" >{getProjectDetail()}</div>
    );
};

export default withTranslation()(ProjectDetails);