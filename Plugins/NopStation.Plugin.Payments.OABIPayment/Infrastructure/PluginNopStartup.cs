using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Payments.OABIPayment.Services;

namespace NopStation.Plugin.Payments.OABIPayment.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Payments.OABIPayment");
            services.AddScoped<IOABIPaymentService, OABIPaymentService>();
        }

        public void Configure(IApplicationBuilder application) { }

        public int Order => 11;
    }
}
