using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Payments.Paykeeper.Services;

namespace NopStation.Plugin.Payments.Paykeeper.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPaykeeperWebRequest, PaykeeperWebRequest>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 101;
    }
}