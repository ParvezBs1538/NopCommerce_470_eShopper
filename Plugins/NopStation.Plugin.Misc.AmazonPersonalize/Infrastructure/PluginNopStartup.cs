using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AmazonPersonalize.Services;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Infrastructure
{
    internal class PluginNopStartup : INopStartup
    {
        public int Order => 1000;

        public void Configure(IApplicationBuilder application)
        {
            application.UseEndpoints(routes =>
            {
                routes.MapHub<InteractionsUploadHub>("/uploadinteractions");
            });
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Misc.AmazonPersonalize");
            services.AddSignalR(hubOptions =>
            {
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
            });

            services.AddScoped<Factories.IEventTrackerModelFactory, Factories.EventTrackerModelFactory>();
            services.AddScoped<Factories.IPersonalizedRecommendationsModelFactory, Factories.PersonalizedRecommendationsModelFactory>();
            services.AddScoped<Factories.IAmazonPersonalizeHelperFactory, Factories.AmazonPersonalizeHelperFactory>();

            services.AddScoped<IAmazonPersonalizeModelFactory, AmazonPersonalizeModelFactory>();
            services.AddScoped<IEventTrackerService, EventTrackerService>();
            services.AddScoped<IPersonalizedRecommendationsService, PersonalizedRecommendationsService>();
            services.AddScoped<IRecommendationService, RecommendationService>();
            services.AddScoped<IRecommenderModelFactory, RecommenderModelFactory>();
            services.AddScoped<IRecommenderService, RecommenderService>();
            services.AddScoped<IEventReportService, EventReportService>();
            services.AddScoped<IPersonalizeExportManager, PersonalizeExportManager>();
            services.AddScoped<InteractionsUploadHub, InteractionsUploadHub>();
        }
    }
}