using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.SEOExpert.Services;

namespace NopStation.Plugin.Widgets.SEOExpert.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public int Order => 100;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.SEOExpert");

            services.AddScoped<ISEOExpertService, SEOExpertService>();
        }
    }
}
