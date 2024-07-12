using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.NopChat.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.NopChat.Factories;
using NopStation.Plugin.Widgets.NopChat.Hubs;
using NopStation.Plugin.Widgets.NopChat.Services;

namespace NopStation.Plugin.Widgets.NopChat.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.NopChat");

            services.AddScoped<NopChatHub, NopChatHub>();
            services.AddScoped<NopChatTestHub, NopChatTestHub>();
            services.AddScoped<INopChatMessageService, NopChatMessageService>();

            services.AddScoped<INopChatModelFactory, NopChatModelFactory>();
            services.AddScoped<INopChatMessageModelFactory, NopChatMessageModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}