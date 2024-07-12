using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Controllers;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Payments.Afterpay.Controllers;
using NopStation.Plugin.Payments.Afterpay.Services;

namespace NopStation.Plugin.Payments.Afterpay.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices(AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME);

            services.AddScoped<IAfterpayUpdateService, AfterpayUpdateService>();
            services.AddScoped<IAfterpayPaymentService, AfterpayPaymentService>();
            services.AddScoped<ShoppingCartController, OverridenShoppingCartController>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => int.MaxValue;
    }
}