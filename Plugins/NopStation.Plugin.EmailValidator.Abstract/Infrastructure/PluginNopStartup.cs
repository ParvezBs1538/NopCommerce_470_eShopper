using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.EmailValidator.Abstract.Factories;
using NopStation.Plugin.EmailValidator.Abstract.Filters;
using NopStation.Plugin.EmailValidator.Abstract.Services;

namespace NopStation.Plugin.EmailValidator.Abstract.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(config =>
            {
                config.Filters.Add(new ValidatorFilter());
            });

            services.AddScoped<IAbstractEmailModelFactory, AbstractEmailModelFactory>();

            services.AddScoped<IAbstractEmailService, AbstractEmailService>();
            services.AddScoped<IValidationService, ValidationService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}