using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.EmailValidator.Verifalia.Factories;
using NopStation.Plugin.EmailValidator.Verifalia.Filters;
using NopStation.Plugin.EmailValidator.Verifalia.Services;

namespace NopStation.Plugin.EmailValidator.Verifalia.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(config =>
            {
                config.Filters.Add(new ValidatorFilter());
            });

            services.AddScoped<IVerifaliaEmailModelFactory, VerifaliaEmailModelFactory>();

            services.AddScoped<IVerifaliaEmailService, VerifaliaEmailService>();
            services.AddScoped<IValidationService, ValidationService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}