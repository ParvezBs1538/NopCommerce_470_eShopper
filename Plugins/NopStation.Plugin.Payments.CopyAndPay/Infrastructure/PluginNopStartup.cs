using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Payments.CopyAndPay.Services;

namespace NopStation.Plugin.Payments.CopyAndPay.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICopyAndPayServices, CopyAndPayServices>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 2;
    }
}
