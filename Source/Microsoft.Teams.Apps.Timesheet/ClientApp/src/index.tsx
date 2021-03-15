// <copyright file="index.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import * as ReactDOM from "react-dom";
import { BrowserRouter as Router } from "react-router-dom";
import App from "./app";

ReactDOM.render(
    <Router>
	    <App />
    </Router>, document.getElementById("root")
);
