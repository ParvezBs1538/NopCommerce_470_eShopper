using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.CrawlerManager.Factories;
using NopStation.Plugin.Widgets.CrawlerManager.Services;

namespace NopStation.Plugin.Widgets.CrawlerManager.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICrawlerService, CrawlerService>();
            services.AddScoped<ICrawlerFactory, CrawlerFactory>();

            services.AddNopStationServices("NopStation.Plugin.Widgets.CrawlerManager", true);
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 3000;
    }
}
