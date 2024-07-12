using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Media;
using NopStation.Plugin.Misc.TinyPNG.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;

namespace NopStation.Plugin.Misc.TinyPNG.Infrastructure
{
    public class NopPluginStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITinyPNGService, TinyPNGService>();
            services.AddScoped<IPictureService, TinyPNGPictureService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => int.MaxValue;
    }
}