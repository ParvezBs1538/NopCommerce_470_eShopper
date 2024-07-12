using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Services;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Factories;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.ProductQuestionAnswer");
            services.AddScoped<IProductQAService, ProductQAService>();
            services.AddScoped<Factories.IProductQAModelFactory, Factories.ProductQAModelFactory>();
            services.AddScoped<IProductQAModelFactory, ProductQAModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 12;
    }
}
