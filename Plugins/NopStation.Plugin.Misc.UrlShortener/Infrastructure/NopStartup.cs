using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.UrlShortener.Factories;
using NopStation.Plugin.Misc.UrlShortener.Services;

namespace NopStation.Plugin.Misc.UrlShortener.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public int Order => 11011;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Misc.UrlShortener");

            services.AddScoped<IShortenUrlModelFactory, ShortenUrlModelFactory>();
            services.AddScoped<IShortenUrlService, ShortenUrlService>();
        }
    }
}
