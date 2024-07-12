using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.StoreLocator.Services;

namespace NopStation.Plugin.Widgets.StoreLocator.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.StoreLocator");

            services.AddScoped<IStoreLocationService, StoreLocationService>();

            services.AddScoped<IStoreLocatorModelFactorey, StoreLocatorModelFactorey>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}