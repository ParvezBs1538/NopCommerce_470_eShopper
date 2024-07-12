using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.AddressValidator.EasyPost.Filters;
using NopStation.Plugin.AddressValidator.EasyPost.Services;

namespace NopStation.Plugin.AddressValidator.EasyPost.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(config =>
            {
                config.Filters.Add(new EasyPostAddressValidatorFilter());
            });

            services.AddScoped<IAddressExtensionService, AddressExtensionService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}