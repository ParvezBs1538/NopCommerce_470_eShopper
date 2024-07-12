using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.CRM.Zoho.Services;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.CRM.Zoho.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices(ZohoDefaults.PluginDirectory, excludepublicView: true);
            services.AddScoped<ZohoSyncHub, ZohoSyncHub>();

            services.AddScoped<IZohoService, ZohoService>();
            services.AddScoped<IUpdatableItemService, UpdatableItemService>();
            services.AddScoped<IMappingService, MappingService>();

            services.AddSignalR(hubOptions =>
            {
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
            });
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseEndpoints(routes =>
            {
                routes.MapHub<ZohoSyncHub>("/synczohoitems");
            });
        }

        public int Order => 1100; //UseEndpoints should be loaded last
    }
}