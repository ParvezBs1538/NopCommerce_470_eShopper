using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DynamicSurvey.Services;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices(DynamicSurveyDefaults.SYSTEM_NAME);

            services.AddScoped<ISurveyModelFactory, SurveyModelFactory>();
            services.AddScoped<ISurveyAttributeModelFactory, SurveyAttributeModelFactory>();

            services.AddScoped<ISurveyService, SurveyService>();
            services.AddScoped<ICopySurveyService, CopySurveyService>();
            services.AddScoped<ISurveyAttributeService, SurveyAttributeService>();
            services.AddScoped<ISurveyAttributeParser, SurveyAttributeParser>();
            services.AddScoped<ISurveyAttributeFormatter, SurveyAttributeFormatter>();

            services.AddScoped<Factories.ISurveyModelFactory, Factories.SurveyModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}