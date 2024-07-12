using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Factories;
using NopStation.Plugin.Shipping.VendorAndState.Services;

namespace NopStation.Plugin.Shipping.VendorAndState.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Shipping.VendorAndState", excludepublicView: true);

            services.AddScoped<IVendorShippingService, VendorShippingService>();
            services.AddScoped<IVendorStateProvinceShippingService, VendorStateProvinceShippingService>();

            services.AddScoped<IVendorShippingFactory, VendorShippingFactory>();
            services.AddScoped<IVendorStateProvinceShippingFactory, VendorStateProvinceShippingFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}