using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Widgets.NopChat.Hubs;

namespace NopStation.Plugin.Widgets.NopChat
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseEndpoints(routes =>
            {
                routes.MapHub<NopChatHub>("/nopChatHub");
                routes.MapHub<NopChatTestHub>("/nopChatTestHub");
            });
        }

        public int Order => 1000; //UseEndpoints should be loaded last
    }
}
