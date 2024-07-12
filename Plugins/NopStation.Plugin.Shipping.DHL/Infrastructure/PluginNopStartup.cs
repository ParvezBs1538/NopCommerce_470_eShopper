using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Shipping.DHL.Areas.Admin.Factories;
using NopStation.Plugin.Shipping.DHL.Services;

namespace NopStation.Plugin.Shipping.DHL.Infrastructure
{
    public record PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Shipping.DHL");

            services.AddScoped<IDHLAcceptedServicesService, DHLAcceptedServicesService>();
            services.AddScoped<IDHLShipmentService, DHLShipmentService>();
            services.AddScoped<IDHLPickupRequestService, DHLPickupRequestService>();
            services.AddScoped<IDHLOrderService, DHLOrderService>();

            services.AddScoped<IDHLModelFactory, DHLModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}