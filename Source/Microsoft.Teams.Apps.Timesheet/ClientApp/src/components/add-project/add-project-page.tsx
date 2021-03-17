// <copyright file="add-project-page.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { Flex, Provider, Loader } from '@fluentui/react-northstar'
import { TFunction } from "i18next";
import IProject from "../../models/project";
import Step1 from "./step1";
import Step2 from "./step2";
import { Guid } from "guid-typescript";
import { AddProjectUISteps } from "../../constants/constants";

import "./add-project.scss";

interface IAddProjectPageState {
    currentStep: number,
    isLoading: boolean,
    project: IProject
}

// Acts as parent for rendering steps used while creating a project
class AddProjectPage extends React.Component<WithTranslation, IAddProjectPageState> {
    readonly localize: TFunction;
    params: { projectId?: string | undefined } = { projectId: undefined };

    // Constructor which initializes state
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;

        this.state = {
            currentStep: 1,
            isLoading: false,
            project: {
                billableHours: 0,
                clientName: "",
                endDate: new Date(),
                nonBillableHours: 0,
                startDate: new Date(),
                tasks: [],
                title: "",
                id: Guid.createEmpty().toString(),
                members: []
            }
        };
    }

    // Updates project state and selects provided step for rendering
    private navigateToStep = (project: IProject, step: number) => {
        this.setState({ project: project, currentStep: step });
    }

    // Renders component based on Add, Edit or Delete
    private renderStep = () => {
        if (this.state.currentStep === AddProjectUISteps.step2) {
            return <Step2 project={this.state.project} onBackClick={this.navigateToStep} />
        }

        return <Step1 project={this.state.project} onNextClick={this.navigateToStep} />
    }

    // Renders the component
    render() {
        return (
            <Provider>
                <Flex>
                    <div className="task-module-container add-project-page">
                        {this.state.isLoading ? <Loader className="loader" /> : this.renderStep()}
                    </div>
                </Flex>
            </Provider>
        );
    }
}

export default withTranslation()(AddProjectPage);