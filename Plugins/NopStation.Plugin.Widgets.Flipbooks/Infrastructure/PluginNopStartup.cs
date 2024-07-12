using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Flipbooks.Services;

namespace NopStation.Plugin.Widgets.Flipbooks.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.Flipbooks");

            services.AddScoped<IFlipbookService, FlipbookService>();

            services.AddScoped<Factories.IFlipbookModelFactory, Factories.FlipbookModelFactory>();

            services.AddScoped<IFlipbookModelFactory, FlipbookModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}