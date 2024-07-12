using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Payments.Unzer.Services;

namespace NopStation.Plugin.Payments.Unzer.Infrastructure
{
    public class NopPluginStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnzerHelperService, UnzerHelperService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 1;
    }
}