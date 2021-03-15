// <copyright file="FakeTimesheetContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Fakes
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;

    /// <summary>
    /// Class to fake Timesheet Context.
    /// </summary>
    public class FakeTimesheetContext
    {
        /// <summary>
        /// Gets fake timesheet context.
        /// </summary>
        /// <returns>Fake timesheet context.</returns>
        public static TimesheetContext GetFakeTimesheetContext()
        {
            var options = new DbContextOptionsBuilder<TimesheetContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new TimesheetContext(options);
        }
    }
}
