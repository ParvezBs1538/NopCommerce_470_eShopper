using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProductRequests.Services;

namespace NopStation.Plugin.Widgets.ProductRequests.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.ProductRequests");

            services.AddScoped<IProductRequestService, ProductRequestService>();
            services.AddScoped<Factories.IProductRequestModelFactory, Factories.ProductRequestModelFactory>();

            services.AddScoped<IProductRequestModelFactory, ProductRequestModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}