using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.IpFilter.Factories;
using NopStation.Plugin.Misc.IpFilter.Services;

namespace NopStation.Plugin.Misc.IpFilter.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Misc.IpFilter", true, true);

            services.AddScoped<IIpBlockRuleService, IpBlockRuleService>();
            services.AddScoped<IIpRangeBlockRuleService, IpRangeBlockRuleService>();
            services.AddScoped<ICountryBlockRuleService, CountryBlockRuleService>();

            services.AddScoped<IIpBlockRuleModelFactory, IpBlockRuleModelFactory>();
            services.AddScoped<IIpRangeBlockRuleModelFactory, IpRangeBlockRuleModelFactory>();
            services.AddScoped<ICountryBlockRuleModelFactory, CountryBlockRuleModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}