using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AlgoliaSearch.Services;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Misc.AlgoliaSearch");

            services.AddSignalR(hubOptions =>
            {
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
            });

            services.AddScoped<IAlgoliaSearchModelFactory, AlgoliaSearchModelFactory>();

            services.AddScoped<ProductUploadHub, ProductUploadHub>();

            services.AddScoped<IAlgoliaUpdatableItemService, AlgoliaUpdatableItemService>();
            services.AddScoped<IAlgoliaCatalogService, AlgoliaCatalogService>();

            services.AddScoped<Factories.IAlgoliaHelperFactory, Factories.AlgoliaHelperFactory>();
            services.AddScoped<Factories.IAlgoliaCatalogModelFactory, Factories.AlgoliaCatalogModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseEndpoints(routes =>
            {
                routes.MapHub<ProductUploadHub>("/uploadproducts");
            });
        }

        public int Order => 1000; //UseEndpoints should be loaded last
    }
}