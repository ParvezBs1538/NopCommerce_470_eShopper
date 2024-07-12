using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Payments.PayHere.Services;

namespace NopStation.Plugin.Payments.PayHere.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<PayHereManager>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order
        {
            get { return 4; }
        }
    }
}
