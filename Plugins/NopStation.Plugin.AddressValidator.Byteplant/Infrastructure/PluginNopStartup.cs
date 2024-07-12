using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.AddressValidator.Byteplant.Filters;
using NopStation.Plugin.AddressValidator.Byteplant.Services;

namespace NopStation.Plugin.AddressValidator.Byteplant.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(config =>
            {
                config.Filters.Add(new ByteplantAddressValidatorFilter());
            });

            services.AddScoped<IAddressExtensionService, AddressExtensionService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}