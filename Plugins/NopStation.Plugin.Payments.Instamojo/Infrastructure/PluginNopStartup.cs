using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Payments.Instamojo.Services;

namespace NopStation.Plugin.Payments.Instamojo.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<InstamojoManager>();
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
