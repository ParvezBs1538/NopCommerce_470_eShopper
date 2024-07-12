using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.PushNop.Services;

namespace NopStation.Plugin.Widgets.PushNop.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.PushNop", excludepublicView: true);

            services.AddScoped<ISmartGroupService, SmartGroupService>();
            services.AddScoped<ISmartGroupNotificationService, SmartGroupNotificationService>();
            services.AddScoped<IPushNopDeviceService, PushNopDeviceService>();

            services.AddScoped<ISmartGroupModelFactory, SmartGroupModelFactory>();
            services.AddScoped<ISmartGroupNotificationModelFactory, SmartGroupNotificationModelFactory>();
            services.AddScoped<IPushNotificationHomeFactory, PushNotificationHomeFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}