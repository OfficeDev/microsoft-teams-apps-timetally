// <copyright file="AuthenticationMetadataControllerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Controllers;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Controller to handle authentication API operations.
    /// </summary>
    [TestClass]
    public class AuthenticationMetadataControllerTests
    {
        /// <summary>
        /// Mocked instance of logger.
        /// </summary>
        private Mock<ILogger<AuthenticationMetadataController>> logger;

        /// <summary>
        /// Holds the instance of authentication meta data controller.
        /// </summary>
        private AuthenticationMetadataController controller;

        /// <summary>
        /// Initializes all test variables.
        /// </summary>
        [TestInitialize]
        public void AuthenticationMetadataControllerTestSetup()
        {
            this.logger = new Mock<ILogger<AuthenticationMetadataController>>();
            this.controller = new AuthenticationMetadataController(this.logger.Object, TestData.AzureSettings, TestData.BotOptions);

            var httpContext = FakeHttpContext.MakeFakeContext();
            this.controller.ControllerContext = new ControllerContext();
            this.controller.ControllerContext.HttpContext = httpContext;
        }

        /// <summary>
        /// Tests whether we get consent URL.
        /// </summary>
        [TestMethod]
        public void GetConsentUrl_WithValidParams_ShouldReturnNotNullResult()
        {
            var okResult = this.controller.GetConsentUrl("Test", "Test");
            Assert.IsNotNull(okResult);
        }
    }
}
