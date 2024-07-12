using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Payments.NetsEasy.Services;

namespace NopStation.Plugin.Payments.NetsEasy.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public int Order => 100;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices(NetsEasyPaymentDefaults.PluginSystemName);

            services.AddScoped<INetsEasyPaymentService, NetsEasyPaymentService>();
        }
    }
}