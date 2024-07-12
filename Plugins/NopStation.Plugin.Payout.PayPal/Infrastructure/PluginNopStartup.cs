using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Payout.PayPal.Services;

namespace NopStation.Plugin.Payout.Stripe.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Payout.PayPal");
            services.AddScoped<IVendorPayPalConfigurationService, VendorPayPalConfigurationService>();
            services.AddScoped<IVendorPayPalPayoutService, VendorPayPalPayoutService>();
        }
        public void Configure(IApplicationBuilder application)
        {
        }
        public int Order => 11;
    }
}