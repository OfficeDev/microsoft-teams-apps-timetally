// <copyright file="ServicesExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Bot;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Models.Configuration;
    using Microsoft.Teams.Apps.Timesheet.Services;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;

    /// <summary>
    /// Class to extend ServiceCollection.
    /// </summary>
    public static class ServicesExtension
    {
        /// <summary>
        /// Adds application configuration settings to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BotSettings>(options =>
            {
                options.AppBaseUri = configuration.GetValue<string>("App:AppBaseUri");
                options.ManifestId = configuration.GetValue<string>("App:ManifestId");
                options.MicrosoftAppId = configuration.GetValue<string>("App:Id");
                options.MicrosoftAppPassword = configuration.GetValue<string>("App:Password");
                options.TimesheetFreezeDayOfMonth = configuration.GetValue<int>("App:TimesheetFreezeDayOfMonth");
                options.DailyEffortsLimit = configuration.GetValue<int>("App:DailyEffortsLimit");
                options.WeeklyEffortsLimit = configuration.GetValue<int>("App:WeeklyEffortsLimit");
                options.CardCacheDurationInHour = configuration.GetValue<int>("App:CardCacheDurationInHour");
                options.ManagerProjectValidationCacheDurationInHours = configuration.GetValue<int>("App:ManagerProjectValidationCacheDurationInHours");
                options.ManagerReporteesCacheDurationInHours = configuration.GetValue<int>("App:ManagerReporteesCacheDurationInHours");
                options.UserPartOfProjectsCacheDurationInHour = configuration.GetValue<int>("App:UserPartOfProjectsCacheDurationInHour");
                options.ManagerReporteesCacheDurationInHours = configuration.GetValue<int>("App:ManagerReporteesCacheDurationInHours");
            });

            services.Configure<AzureSettings>(options =>
            {
                options.TenantId = configuration.GetValue<string>("AzureAd:TenantId");
                options.ClientId = configuration.GetValue<string>("App:Id");
                options.ApplicationIdURI = configuration.GetValue<string>("AzureAd:ApplicationIdURI");
                options.ValidIssuers = configuration.GetValue<string>("AzureAd:ValidIssuers");
                options.Instance = configuration.GetValue<string>("AzureAd:Instance");
                options.GraphScope = configuration.GetValue<string>("AzureAd:GraphScope");
            });

            services.Configure<AzureSettings>(options =>
            {
                options.TenantId = configuration.GetValue<string>("AzureAd:TenantId");
                options.ClientId = configuration.GetValue<string>("App:Id");
                options.ApplicationIdURI = configuration.GetValue<string>("AzureAd:ApplicationIdURI");
                options.ValidIssuers = configuration.GetValue<string>("AzureAd:ValidIssuers");
                options.Instance = configuration.GetValue<string>("AzureAd:Instance");
                options.GraphScope = configuration.GetValue<string>("AzureAd:GraphScope");
            });
        }

        /// <summary>
        /// Registers repositories for DB operations.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddTransient<IRepositoryAccessors, RepositoryAccessors>();
        }

        /// <summary>
        /// Registers helpers for DB operations.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterHelpers(this IServiceCollection services)
        {
            services.AddTransient<IProjectHelper, ProjectHelper>();
            services.AddTransient<IAppLifecycleHandler, AppLifecycleHandler>();
            services.AddTransient<IProjectHelper, ProjectHelper>();
            services.AddTransient<IManagerDashboardHelper, ManagerDashboardHelper>();
            services.AddTransient<IUserHelper, UserHelper>();
            services.AddTransient<ITimesheetHelper, TimesheetHelper>();
            services.AddTransient<IManagerDashboardHelper, ManagerDashboardHelper>();
            services.AddTransient<IUserHelper, UserHelper>();
            services.AddTransient<ITaskHelper, TaskHelper>();
            services.AddTransient<INotificationHelper, NotificationHelper>();
        }

        /// <summary>
        /// Registers services for mapping models.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterModelMappers(this IServiceCollection services)
        {
            services.AddScoped<IManagerDashboardMapper, ManagerDashboardMapper>();
            services.AddScoped<IProjectMapper, ProjectMapper>();
            services.AddScoped<IMemberMapper, MemberMapper>();
            services.AddScoped<ITaskMapper, TaskMapper>();
        }

        /// <summary>
        /// Registers services such as MS Graph, token acquisition etc.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterGraphServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationProvider, GraphTokenProvider>();
            services.AddScoped<IGraphServiceClient, GraphServiceClient>();
            services.AddScoped<IGraphServiceFactory, GraphServiceFactory>();
            services.AddScoped<IUsersService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetUsersService());
        }

        /// <summary>
        /// Adds services to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IProjectMapper, ProjectMapper>();
            services.AddScoped<ITimesheetMapper, TimesheetMapper>();
            services.AddScoped<ITaskMapper, TaskMapper>();
            services.AddSingleton<IAdaptiveCardService, AdaptiveCardService>();
        }

        /// <summary>
        /// Adds credential providers for authentication.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterCredentialProviders(this IServiceCollection services, IConfiguration configuration)
        {
            ICredentialProvider credentialProvider = new SimpleCredentialProvider(
                appId: configuration.GetValue<string>("App:Id"),
                password: configuration.GetValue<string>("App:Password"));

            services
                .AddSingleton(credentialProvider);
        }

        /// <summary>
        /// Add confidential credential provider to access API.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterConfidentialCredentialProvider(this IServiceCollection services, IConfiguration configuration)
        {
            configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            IConfidentialClientApplication confidentialClientApp = ConfidentialClientApplicationBuilder.Create(configuration["App:Id"])
                .WithClientSecret(configuration["App:Password"])
                .Build();
            services.AddSingleton<IConfidentialClientApplication>(confidentialClientApp);
        }

        /// <summary>
        /// Adds localization settings to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterLocalizationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = CultureInfo.GetCultureInfo(configuration.GetValue<string>("i18n:DefaultCulture"));
                var supportedCultures = configuration.GetValue<string>("i18n:SupportedCultures").Split(',')
                    .Select(culture => CultureInfo.GetCultureInfo(culture))
                    .ToList();

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new TimesheetLocalizationCultureProvider(),
                };
            });
        }
    }
}