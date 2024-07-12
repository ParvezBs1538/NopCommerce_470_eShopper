using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.FAQ.Services;

namespace NopStation.Plugin.Widgets.FAQ.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.FAQ");

            services.AddScoped<IFAQCategoryService, FAQCategoryService>();
            services.AddScoped<IFAQItemService, FAQItemService>();
            services.AddScoped<IFAQTagService, FAQTagService>();

            services.AddScoped<Factories.IFAQItemModelFactory, Factories.FAQItemModelFactory>();

            services.AddScoped<IFAQCategoryModelFactory, FAQCategoryModelFactory>();
            services.AddScoped<IFAQItemModelFactory, FAQItemModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}