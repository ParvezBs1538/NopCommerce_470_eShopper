using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Payments.CBL.Services;

namespace NopStation.Plugin.Payments.CBL.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices(CBLPaymentDefaults.PLUGIN_SYSTEM_NAME);

            services.AddScoped<ICBLPaymentService, CBLPaymentService>();
        }
        public void Configure(IApplicationBuilder application)
        {
        }
        public int Order => int.MaxValue;
    }
}