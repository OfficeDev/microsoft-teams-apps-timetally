// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

[assembly: Microsoft.Azure.Functions.Extensions.DependencyInjection.FunctionsStartup(
    typeof(Microsoft.Teams.Apps.Timesheet.ReminderFunction.Startup))]

namespace Microsoft.Teams.Apps.Timesheet.ReminderFunction
{
    using System;
    using System.Globalization;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Common.Services.CommonBot;
    using Microsoft.Teams.Apps.Timesheet.Common.Services.Message;

    /// <summary>
    /// Register services in DI container of the Azure functions system.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configure the Dependency Injection Container (Composition Root).
        /// </summary>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder" />.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.ConfigureServices(builder.Services);
        }

        /// <summary>
        /// Configure the Dependency Injection Container (Composition Root).
        /// </summary>
        /// <param name="services">The DI Container.</param>
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<RepositoryOptions>()
                .Configure<IConfiguration>((repositoryOptions, configuration) =>
                {
                    repositoryOptions.StorageAccountConnectionString =
                        configuration.GetValue<string>("StorageAccountConnectionString");
                });

            services.AddOptions<FunctionOptions>()
                .Configure<IConfiguration>((repositoryOptions, configuration) =>
                {
                    repositoryOptions.AppBaseUri =
                        configuration.GetValue<string>("AppBaseUri");

                    repositoryOptions.ManifestId = configuration.GetValue<string>("ManifestId");
                });

            services.AddOptions<BotOptions>()
                .Configure<IConfiguration>((botOptions, configuration) =>
                {
                    botOptions.MicrosoftAppId =
                        configuration.GetValue<string>("MicrosoftAppId");

                    botOptions.MicrosoftAppPassword =
                        configuration.GetValue<string>("MicrosoftAppPassword");
                });

            services.AddDbContext<TimesheetContext>(options =>
                options.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionString")));

            services.AddTransient<IRepositoryAccessors, RepositoryAccessors>();

            services.AddLocalization();

            // Set current culture.
            var culture = Environment.GetEnvironmentVariable("i18n:DefaultCulture");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);

            // Message services for sending notifications.
            services.AddTransient<IMessageService, MessageService>();

            // Add bot services.
            services.AddSingleton<CommonMicrosoftAppCredentials>();
            services.AddSingleton<ICredentialProvider, CommonBotCredentialProvider>();
            services.AddSingleton<BotFrameworkHttpAdapter>();
        }
    }
}