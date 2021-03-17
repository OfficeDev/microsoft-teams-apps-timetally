// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet
{
    using System;
    using System.Globalization;
    using global::Azure.Identity;
    using global::Azure.Security.KeyVault.Secrets;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Bot;
    using Microsoft.Teams.Apps.Timesheet.Common.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Models.Configuration;

    /// <summary>
    /// The Startup class is responsible for configuring the DI container and acts as the composition root.
    /// </summary>
    public sealed class Startup
    {
        /// <summary>
        /// The day of month on which timesheet get frozen.
        /// </summary>
        private const int TimesheetFreezeDayOfMonth = 10;

        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The environment provided configuration.</param>
#pragma warning disable SA1201 // Declare property before initializing in constructor
        public Startup(IConfiguration configuration)
#pragma warning restore SA1201 // Declare property before initializing in constructor
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var useKeyVault = this.configuration.GetValue<bool>("UseKeyVault");

            if (useKeyVault)
            {
                this.GetKeyVaultByManagedServiceIdentity();
            }

            this.ValidateConfigurationSettings();
        }

        /// <summary>
        /// Configure the composition root for the application.
        /// </summary>
        /// <param name="services">The stub composition root.</param>
        /// <remarks>
        /// For more information see: https://go.microsoft.com/fwlink/?LinkID=398940.
        /// </remarks>
#pragma warning disable CA1506 // Composition root expected to have coupling with many components.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(
                new MicrosoftAppCredentials(
                     this.configuration.GetValue<string>("App:Id"),
                     this.configuration.GetValue<string>("App:Password")));

            services.RegisterConfigurationSettings(this.configuration);
            services.AddControllers();
            services.AddMvc().AddMvcOptions(mvcopt => { mvcopt.EnableEndpointRouting = false; });
            services.AddHttpContextAccessor();
            services.AddSingleton<IChannelProvider, SimpleChannelProvider>();
            services.AddSingleton<IMemoryCache, MemoryCache>();

            services.RegisterCredentialProviders(this.configuration);
            services.RegisterConfidentialCredentialProvider(this.configuration);
            services.RegisterRepositories();
            services.RegisterAuthenticationServices(this.configuration);

            // Add Microsoft Graph services.
            services.RegisterGraphServices();
            services.RegisterHelpers();
            services.RegisterModelMappers();
            services.AddSingleton<TelemetryClient>();

            services
                .AddApplicationInsightsTelemetry(this.configuration.GetValue<string>("ApplicationInsights:InstrumentationKey"));

            services
                .AddTransient<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();

            services
                .AddTransient<IBot, TimesheetActivityHandler>();

            services.AddDbContext<TimesheetContext>(options =>
                options.UseSqlServer(
                this.configuration.GetValue<string>("SQLStorage:ConnectionString")));

            services.RegisterServices();

            // In production, the React files will be served from this directory.
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.RegisterLocalizationSettings(this.configuration);
        }
#pragma warning restore CA1506

        /// <summary>
        /// Configure the application request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">Hosting Environment.</param>
        /// <param name="timesheetContext">The timesheet context.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TimesheetContext timesheetContext)
        {
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMvc();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.EnvironmentName.ToUpperInvariant() == "DEVELOPMENT")
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

#pragma warning disable CA1062 // The 'timesheetContext' will be available through dependency injection.
            timesheetContext.Database.Migrate();
#pragma warning restore CA1062 // The 'timesheetContext' will be available through dependency injection.
        }

        /// <summary>
        /// Validate whether the configuration settings are missing or not.
        /// </summary>
        private void ValidateConfigurationSettings()
        {
            var azureSettings = new AzureSettings();
            this.configuration.Bind("AzureAd", azureSettings);
            azureSettings.ClientId = this.configuration.GetValue<string>("App:Id");

            if (string.IsNullOrWhiteSpace(azureSettings.ClientId))
            {
                throw new ApplicationException("AzureAD ClientId is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(azureSettings.TenantId))
            {
                throw new ApplicationException("AzureAD TenantId is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(azureSettings.ApplicationIdURI))
            {
                throw new ApplicationException("AzureAD ApplicationIdURI is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(azureSettings.ValidIssuers))
            {
                throw new ApplicationException("AzureAD ValidIssuers is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(this.configuration.GetValue<string>("App:ManifestId")))
            {
                throw new ApplicationException("Manifest Id is missing in the configuration file.");
            }

            if (this.configuration.GetValue<int?>("App:CardCacheDurationInHour") == null || this.configuration.GetValue<int>("App:CardCacheDurationInHour") < 1)
            {
                throw new ApplicationException("Invalid card cache duration value in configuration file. The value must be at least 1 hour.");
            }

            if (this.configuration.GetValue<int?>("App:TimesheetFreezeDayOfMonth") == null
                || this.configuration.GetValue<int>("App:TimesheetFreezeDayOfMonth") < 1
                || this.configuration.GetValue<int>("App:TimesheetFreezeDayOfMonth") > 31)
            {
                this.configuration["App:TimesheetFreezeDayOfMonth"] = TimesheetFreezeDayOfMonth.ToString(CultureInfo.InvariantCulture);
            }

            if (this.configuration.GetValue<int?>("App:WeeklyEffortsLimit") == null || this.configuration.GetValue<int>("App:WeeklyEffortsLimit") < 1)
            {
                throw new ApplicationException("Invalid weekly efforts value in configuration file. The value must be at least 1 hour.");
            }

            if (this.configuration.GetValue<int?>("App:UserPartOfProjectsCacheDurationInHour") == null || this.configuration.GetValue<int>("App:UserPartOfProjectsCacheDurationInHour") < 1)
            {
                throw new ApplicationException("Invalid user part of projects cache duration value in configuration file. The value must be at least 1 hour.");
            }
        }

        /// <summary>
        /// Get KeyVault secrets and app settings values.
        /// </summary>
        private void GetKeyVaultByManagedServiceIdentity()
        {
            // Create a new secret client using the default credential from Azure.Identity using environment variables.
            var client = new SecretClient(
                vaultUri: new Uri($"{this.configuration["KeyVaultUrl:BaseURL"]}/"),
                credential: new DefaultAzureCredential());

            this.configuration["AzureAd:ClientId"] = this.configuration["App:Id"] = client.GetSecret("MicrosoftAppId--SecretKey").Value.Value;
            this.configuration["AzureAd:ClientSecret"] = this.configuration["App:Password"] = client.GetSecret("MicrosoftAppPassword--SecretKey").Value.Value;
            this.configuration["SQLStorage:ConnectionString"] = client.GetSecret("SQLStorageConnectionString--SecretKey").Value.Value;
        }
    }
}