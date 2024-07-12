using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Payments.MPay24.Areas.Admin.Factories;
using NopStation.Plugin.Payments.MPay24.Services;

namespace NopStation.Plugin.Payments.MPay24.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Payments.MPay24");

            services.AddScoped<IPaymentOptionModelFactory, PaymentOptionModelFactory>();
            services.AddScoped<IPaymentOptionService, PaymentOptionService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}