using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Payments.BlueSnapHosted.Services;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IBlueSnapServices, BlueSnapServices>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 2;
    }
}