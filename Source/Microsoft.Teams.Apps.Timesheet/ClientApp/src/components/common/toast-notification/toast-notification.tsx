// <copyright file="toast-notification.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { Flex, BookmarkIcon, Text, CloseIcon } from "@fluentui/react-northstar";
import IToastNotification from "../../../models/toast-notification";
import { ActivityStatus } from "../../../models/activity-status";

import "./toast-notification.scss";

interface IToastNotificationProps {
    isMobile: boolean,
    notification: IToastNotification
}

// The timespan for which notification will be active on screen.
const NotificationActiveTimespan: number = 6000;

/**
 * The toast notification which shows the recent status messages
 * @param props The props of type IToastNotificationProps
 */
const ToastNotification: React.FunctionComponent<IToastNotificationProps> = props => {
    const [showNotification, setShowNotification] = React.useState(false);
    let timeoutId: number = 0;

    React.useEffect(() => {
        if (props.notification.message?.length && props.notification.type !== ActivityStatus.None) {
            setShowNotification(true);

            timeoutId = window.setTimeout(onClose, NotificationActiveTimespan);
        }
    }, [props.notification.id, props.notification.message, props.notification.type]);

    // Event handler called when notification get closed
    function onClose() {
        setShowNotification(false);

        if (timeoutId) {
            window.clearTimeout(timeoutId);
        }
    }

    if (!showNotification) {
        return <></>;
    }

    return (
        <Flex
            className={`notification-toast ${props.notification.type === ActivityStatus.Success ? "success" : "error"}`}
            vAlign="center"
            gap="gap.small"
            hAlign="center">
                {props.isMobile ? <BookmarkIcon /> : null}
            <Text className="notification" content={props.notification.message} weight={props.isMobile ? "regular" : "semibold"} />
            <Flex.Item push>
                <CloseIcon className="cursor-pointer" size="small" onClick={onClose} />
            </Flex.Item>
        </Flex>
    );
}

export default ToastNotification;