using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.WidgetPush.Services;

namespace NopStation.Plugin.Widgets.WidgetPush.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.WidgetPush");

            services.AddScoped<IWidgetPushItemService, WidgetPushItemService>();

            services.AddScoped<IWidgetPushItemModelFactory, WidgetPushItemModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}