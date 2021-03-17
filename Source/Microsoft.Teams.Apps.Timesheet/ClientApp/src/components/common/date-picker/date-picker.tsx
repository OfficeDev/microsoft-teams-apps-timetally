// <copyright file="date-picker.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import moment from "moment";
import { Flex } from '@fluentui/react-northstar';
import { useState } from "react";
import { DatePicker } from 'office-ui-fabric-react';
import { Fabric, Customizer } from 'office-ui-fabric-react';
import { initializeIcons } from 'office-ui-fabric-react';
import { DarkCustomizations } from "./dark-customizations";
import { DefaultCustomizations } from "./default-customizations";
import { Themes } from "../../../constants/constants";
import "./date-picker.scss";

initializeIcons();

interface IDatePickerProps {
    className?: string,
    selectedDate: Date;
    onDateSelect: (startDate: Date) => void,
    theme: string,
    minDate: Date | undefined;
    maxDate: Date | undefined;
    disableSelectionForPastDate: boolean
}

// Renders date-picker with Teams theme support
const DatePickerWrapper: React.FC<IDatePickerProps> = props => {
    let className = "";
    let theme = props.theme;
    let datePickerTheme;

    if (theme === Themes.dark) {
        className = "dark-datepicker";
        datePickerTheme = DarkCustomizations
    }
    else if (theme === Themes.contrast) {
        className = "dark-datepicker";
        datePickerTheme = DarkCustomizations
    }
    else {
        className = "default-datepicker";
        datePickerTheme = DefaultCustomizations
    }

    const [selectedDate, setDate] = useState<Date | undefined>(props.selectedDate);
    const [minDate, setMinDate] = useState<Date | undefined>(props.minDate);
    const [maxDate, setMaxDate] = useState<Date | undefined>(props.maxDate);

    React.useEffect(() => {
        setDate(props.selectedDate);
    }, [props.selectedDate]);

    React.useEffect(() => {
        setMinDate(props.minDate);
    }, [props.minDate]);

    React.useEffect(() => {
        setMaxDate(props.maxDate);
    }, [props.maxDate]);

    /**
    * Format date to show in date picker.
    * @param date Selected date.
    */
    const formatDate = (date: Date | null | undefined): string => {
        return moment(date).format('LL')
    };

    /**
    * Handle change event for cycle start date picker.
    * @param date Selected date.
    */
    const onSelectStartDate = (date: Date | null | undefined): void => {
        let startCycle = moment(date).startOf('day');
        props.onDateSelect(startCycle.toDate()!);
        setDate(startCycle.toDate());
    };

    return (
        <Flex gap="gap.small" className="custom-date-picker">
            <Fabric className={`full-width ${props.className ? props.className : ""}`}>
                <Customizer {...datePickerTheme}>
                    <DatePicker
                        className={className}
                        label={''}
                        showMonthPickerAsOverlay={true}
                        minDate={minDate}
                        maxDate={maxDate}
                        isMonthPickerVisible={true}
                        value={selectedDate}
                        onSelectDate={onSelectStartDate}
                        disabled={props.disableSelectionForPastDate}
                        formatDate={formatDate}
                    />
                </Customizer>
            </Fabric>
        </Flex>
    );
}

export default DatePickerWrapper;