using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Factories;
using NopStation.Plugin.Shipping.Redx.Services;

namespace NopStation.Plugin.Shipping.Redx.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Shipping.Redx", excludepublicView: true);

            services.AddScoped<IRedxShipmentService, RedxShipmentService>();
            services.AddScoped<IRedxAreaService, RedxAreaService>();

            services.AddScoped<IRedxAreaModelFactory, RedxAreaModelFactory>();
            services.AddScoped<IRedxShipmentModelFactory, RedxShipmentModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}