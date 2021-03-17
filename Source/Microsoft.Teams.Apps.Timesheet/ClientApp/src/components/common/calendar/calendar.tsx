// <copyright file="calendar.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import moment from "moment";
import { cloneDeep } from "lodash";
import { TFunction } from "i18next";
import { useTranslation } from "react-i18next";
import { Text, Flex, ChevronStartIcon, ChevronEndIcon, Button, Loader } from "@fluentui/react-northstar";
import ITimesheet from "../../../models/timesheet";
import IUserTimesheet from "../../../models/fill-timesheet/user-timesheet";
import IProjectDetails from "../../../models/fill-timesheet/project-details";
import { TimesheetStatus } from "../../../models/timesheet-status";
import { getTotalEfforts } from "../../../Helpers/common-helper";
import ITimesheetDetails from "../../../models/fill-timesheet/timesheet-details";

import "./calendar.scss";

interface ICalendarProps {
    timesheetData: IUserTimesheet[];
    timesheetFreezeDayOfMonth?: number;
    isManagerView: boolean;
    isMobile: boolean;
    isDisabled: boolean;
    isLoading: boolean;
    isDuplicatingEfforts: boolean;
    onEffortsDuplicated: (selectedWeekdaysToDuplicate: ITimesheet[]) => void;
    onCalendarEditModeChange: (isInEditMode: boolean) => void;
    onCalendarActiveDateChange?: (previousSelectedDate: Date, selectedDate: ITimesheet, isFrozen: boolean) => void;
    onWeekChange: (weekNumber: number) => void;
    onSelectedDatesChange?: (selectedWeekdaysToDuplicate: ITimesheet[]) => void;
}

// Renders calendar component
const Calendar: React.FunctionComponent<ICalendarProps> = props => {
    const localize: TFunction = useTranslation().t;

    const [selectedDate, setSelectedDate] = React.useState(moment().startOf('day').toDate());
    const [renderedWeek, setRenderedWeek] = React.useState(moment().week());
    const [isDuplicateHoursActivated, setDuplicatedHoursActivated] = React.useState(false);
    const [calendarDates, setCalendarDates] = React.useState([] as ITimesheet[]);
    const [selectedWeekdaysToDuplicate, setSelectedWeekdaysToDuplicate] = React.useState([] as ITimesheet[]);
    const [filledEffortsOfRenderedWeek, setFilledEffortsOfRenderedWeek] = React.useState(0);

    React.useEffect(() => {
        getCalendarDates();
    }, [props.timesheetData, renderedWeek]);

    // Gets the calendar dates to load in calendar by week number.
    function getCalendarDates() {
        // Gets start date and end date of a week
        let startDateOfWeek: Date = moment().week(renderedWeek).startOf('week').startOf('day').toDate();
        let endDateOfWeek: Date = moment().week(renderedWeek).endOf('week').startOf('day').toDate();

        let calendarDates: ITimesheet[] = [];
        let filledEffortsOfRenderedWeek: number = 0;

        // Prepares date array that need to be displayed in calendar for a week
        for (let currentDate: Date = startDateOfWeek; currentDate <= endDateOfWeek;) {
            let date: Date = moment(currentDate).startOf('day').toDate();

            // Check whether timesheet data available for a date
            let timesheet = props.timesheetData ? props.timesheetData.find((timesheet: IUserTimesheet) =>
                timesheet.timesheetDate.valueOf() === date.valueOf()) : undefined;

            // As the timesheet filled for that day, copy the same data in calendar dates array
            if (timesheet) {
                calendarDates.push({ date, hours: getTotalEfforts(timesheet), status: getTimesheetStatus(timesheet.projectDetails) });
                filledEffortsOfRenderedWeek += calendarDates[calendarDates.length - 1].hours;
            }
            else {
                // Insert new data in calendar dates array with efforts 0 as timesheet wasn't filled for that day
                calendarDates.push({ date, hours: 0, status: TimesheetStatus.None });
            }

            // Add a day to render next date in calendar
            currentDate = moment(currentDate).add(1, "day").toDate();
        }

        let selectedDateTimesheet = calendarDates.find((timesheet: ITimesheet) =>
            timesheet.date.valueOf() === selectedDate.valueOf());

        if (selectedDateTimesheet && props.onCalendarActiveDateChange) {
            props.onCalendarActiveDateChange(selectedDateTimesheet.date, selectedDateTimesheet, isTimesheetDateFrozen(selectedDateTimesheet.date, moment().startOf('day').toDate()));
        }

        setFilledEffortsOfRenderedWeek(filledEffortsOfRenderedWeek);
        setCalendarDates(calendarDates);
    }

    /**
     * Gets the status for timesheet on particular date.
     * @param projectDetails The projects details along with tasks.
     */
    function getTimesheetStatus(projectDetails: IProjectDetails[]) {
        if (projectDetails) {
            for (let i = 0; i < projectDetails.length; i++) {
                let filledTimesheets = projectDetails[i].timesheetDetails.filter((timesheet: ITimesheetDetails) =>
                    timesheet.status !== TimesheetStatus.None);

                // Manager is allowed to approve/reject timesheets day wise. Hence all the timesheets for particular day
                // will have same status.
                if (filledTimesheets && filledTimesheets.length > 0) {
                    return filledTimesheets[0].status;
                }
            }
        }

        return TimesheetStatus.None;
    }

    // Duplicates efforts for selected date
    function duplicateHours() {
        if (selectedWeekdaysToDuplicate && selectedWeekdaysToDuplicate.length > 0) {
            props.onEffortsDuplicated(selectedWeekdaysToDuplicate);
            setSelectedWeekdaysToDuplicate([]);
        }
    }

    /**
     * Renders the efforts in desired color based on whether the efforts for particular day were accepted or rejected by manager
     * @param timesheetStatus The timesheet status of a day.
     */
    function getEffortsColorStyle(timesheetStatus: TimesheetStatus) {
        switch (timesheetStatus) {
            case TimesheetStatus.Saved:
            case TimesheetStatus.Submitted:
                return "saved-hours";

            case TimesheetStatus.Approved:
                return "approved-hours";

            case TimesheetStatus.Rejected:
                return "rejected-hours";

            default:
                return "default";
        }
    }

    /**
     * Gets the CSS class needed for duplicate efforts functionality which manages styling for date selection and suggest
     * where the efforts can be copied.
     * @param timesheet The timesheet details.
     */
    function getDuplicateHoursSelectionClass(timesheet: ITimesheet) {
        // Highlight and select the date to duplicate efforts only if weekday is valid to duplicate efforts.
        if (canEffortsBeDuplicated(timesheet)) {
            let styleClass: string = props.isMobile ? "cursor-pointer mobile select-to-duplicate" : "cursor-pointer select-to-duplicate";

            let weekdayDetails = selectedWeekdaysToDuplicate.find((weekdayDetails: ITimesheet) => weekdayDetails.date.valueOf() === timesheet.date.valueOf());
            styleClass = styleClass + (weekdayDetails ? " selected-to-duplicate" : "");

            return styleClass;
        }

        return "cursor-pointer";
    }

    /**
     * Indicates whether efforts can be duplicated for a weekday.
     * @param timesheet The weekday details.
     */
    function canEffortsBeDuplicated(timesheet: ITimesheet) {
        // 1. 'Duplicate efforts' functionality is in-progress
        // 2. Rendering date is not selected date
        // 3. Do not allow to select date for duplicate efforts if the calendar date is frozen.
        // 4. Efforts for a date were rejected or not yet approved
        return isDuplicateHoursActivated
            && timesheet.date.valueOf() !== selectedDate.valueOf()
            && !isTimesheetDateFrozen(timesheet.date, moment().toDate())
            && timesheet.status !== TimesheetStatus.Approved;
    }

    /**
     * Determines whether a date rendered in calendar is frozen.
     * @param dateToCheck The date to check.
     * @param currentDate The current date.
     */
    function isTimesheetDateFrozen(dateToCheck: Date, currentDate: Date) {
        if (!props.timesheetFreezeDayOfMonth) {
            return false;
        }

        // Freeze timesheet of upcoming months.
        if (moment(dateToCheck).startOf('day').toDate().valueOf() > moment().endOf('month').startOf('day').toDate().valueOf()) {
            return true;
        }

        // If today's date is greater than or equal to timesheet freezing day of month.
        if (moment().date() >= props.timesheetFreezeDayOfMonth) {
            // If the date belongs to previous month, then timesheet will get freeze and user can fill timesheet from current month.
            return dateToCheck.valueOf() < moment(currentDate).startOf('month').startOf('day').valueOf();
        }

        // Allow user to fill timesheet from previous month.
        return dateToCheck.valueOf() < moment(currentDate).subtract(1, 'months').startOf('month').startOf('day').valueOf();
    }

    // Event handler called when requested to display next week
    function onRequestNextCalendarWeek() {
        props.onWeekChange(renderedWeek + 1);
        setRenderedWeek(renderedWeek + 1);
    }

    // Event handler called when requested to display previous week
    function onRequestPreviousCalendarWeek() {
        props.onWeekChange(renderedWeek - 1);
        setRenderedWeek(renderedWeek - 1);
    }

    // Event handler called when click on duplicate hours
    function onRequestToDuplicateHoursClick() {
        if (isDuplicateHoursActivated) {
            duplicateHours();
        }

        props.onCalendarEditModeChange(!isDuplicateHoursActivated);
        setDuplicatedHoursActivated(!isDuplicateHoursActivated);
    }

    /**
     * The event handler called when a calendar date is selected
     * @param weekday The selected weekday details
     */
    function onCalendarDateSelected(weekday: ITimesheet) {
        if (isDuplicateHoursActivated) {
            if (!canEffortsBeDuplicated(weekday)) {
                return;
            }

            let weekdayDetails = selectedWeekdaysToDuplicate.find((weekdayDetails: ITimesheet) => weekdayDetails.date.valueOf() === weekday.date.valueOf());
            let selectedWeekdays: ITimesheet[] = selectedWeekdaysToDuplicate ? cloneDeep(selectedWeekdaysToDuplicate) : [];

            if (!weekdayDetails) {
                selectedWeekdays.push(weekday);
            }
            else {
                selectedWeekdays = selectedWeekdays.filter((day: ITimesheet) => day.date.valueOf() !== weekday.date.valueOf());
            }

            props.onSelectedDatesChange && props.onSelectedDatesChange(selectedWeekdays);
            setSelectedWeekdaysToDuplicate(selectedWeekdays);
        }
        else {
            let previousSelectedDate = selectedDate;

            setSelectedDate(weekday.date);
            props.onCalendarActiveDateChange && props.onCalendarActiveDateChange(previousSelectedDate, weekday, isTimesheetDateFrozen(weekday.date, moment().startOf('day').toDate()));
        }
    }

    // Renders weekdays as per week
    function renderDays() {
        if (props.isLoading) {
            return <Loader className="loader" size="small" />;
        }

        let todaysDate = moment().startOf('day').toDate();
        let weekdays: ITimesheet[] = calendarDates ?? [];

        let weekdayElements: JSX.Element[] = weekdays.map((weekday: ITimesheet, index: number) => {
            let day = weekday.date.getDay();
            let todaysDateClass = weekday.date.valueOf() === todaysDate.valueOf() ? "todays-date" : "";
            let dateSelectionClass = getDuplicateHoursSelectionClass(weekday);

            if (weekday.date.valueOf() === selectedDate.valueOf()) {
                dateSelectionClass = dateSelectionClass + (props.isMobile ? " mobile selected-date" : " selected-date");
            }

            let colorCodeStyleForEfforts: string = getEffortsColorStyle(weekday.status);

            return (
                <Flex key={`weekday-${index}`} className={dateSelectionClass} column hAlign="center" space="between" data-testid={`calendar-date-${moment(weekday.date).format("YYYY-MM-DD")}`} onClick={() => onCalendarDateSelected(weekday)}>
                    <Text
                        className="week-day"
                        content={props.isMobile ? moment.weekdaysShort(day).charAt(0) : moment.weekdaysShort(day)}
                        size="small"
                        weight={props.isMobile ? "regular" : "semibold"}
                    />
                    <Text
                        className={props.isMobile ? `mobile date-text ${todaysDateClass}` : `date-text ${todaysDateClass}`}
                        content={moment(weekday.date).format("DD")} />
                    {!props.isLoading ?
                        <Text
                            className={props.isMobile ? `mobile hours-text ${colorCodeStyleForEfforts}` : `hours-text ${colorCodeStyleForEfforts}`}
                            content={("0" + weekday.hours).slice(-2)}
                            weight="semibold"
                        /> : <Loader />}
                </Flex>
            );
        });

        return <Flex className="calendar-days" space="around" fill>{weekdayElements}</Flex>;
    }

    // Renders header of calendar
    function renderWeekAndMonthHeader() {
        let date = moment().week(renderedWeek);
        let month = date.format("MMM YYYY");
        return localize("calendarWeekAndMonth", { weekNumber: date.format("WW"), month, filledEffortsOfWeek: ("0" + filledEffortsOfRenderedWeek).slice(-2) });
    }

    // Renders footer of calendar
    function renderCalendarFooter() {
        return (
            <Flex className="calendar-footer" vAlign="center">
                {
                    isDuplicateHoursActivated ? <Text data-testid="duplicate-efforts-placeholder" content={localize("duplicateEffortsPlaceholder")} /> : null
                }
                <Flex.Item push>
                    <div>
                        {!props.isManagerView
                            ? <Button
                                data-testid="duplicate-efforts-button"
                                text
                                primary
                                disabled={props.isDuplicatingEfforts || props.isDisabled || props.isLoading}
                                loading={props.isDuplicatingEfforts}
                                content={isDuplicateHoursActivated ? localize("duplicateEffortsSubmitButtonText") : localize("duplicateEffortsButtonText")}
                                onClick={onRequestToDuplicateHoursClick} />
                            : null}
                    </div>
                </Flex.Item>
            </Flex>
        );
    }

    // Renders component
    return (
        <Flex data-tid={`calendar-component`} className="calendar" column>
            <Flex vAlign="center" space="between">
                <Button text iconOnly icon={<ChevronStartIcon size="small" />} onClick={onRequestPreviousCalendarWeek} />
                <Text content={renderWeekAndMonthHeader()} weight="semibold" />
                <Button text iconOnly icon={<ChevronEndIcon size="small" />} onClick={onRequestNextCalendarWeek} />
            </Flex>
            {renderDays()}
            <Flex.Item push>
                {renderCalendarFooter()}
            </Flex.Item>
        </Flex>
    );
}

export default Calendar;