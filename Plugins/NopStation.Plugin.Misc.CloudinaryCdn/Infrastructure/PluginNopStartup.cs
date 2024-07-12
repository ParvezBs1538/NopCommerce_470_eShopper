using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Media;
using Nop.Web.Framework.UI;
using NopStation.Plugin.Misc.CloudinaryCdn.Services;

namespace NopStation.Plugin.Misc.CloudinaryCdn.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<PictureService>();
            services.AddTransient<CloudinaryPictureService>();
            services.AddTransient<NopHtmlHelper>();
            services.AddTransient<CloudinaryNopHtmlHelper>();

            services.AddScoped<IPictureService>(serviceProvider =>
            {
                if (serviceProvider.GetRequiredService<CloudinaryCdnSettings>().Enabled)
                {
                    return serviceProvider.GetRequiredService<CloudinaryPictureService>();
                }
                else
                {
                    return serviceProvider.GetRequiredService<PictureService>();
                }
            });

            services.AddScoped<INopHtmlHelper>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<CloudinaryCdnSettings>();
                if (settings.Enabled && (settings.EnableCssCdn || settings.EnableJsCdn))
                {
                    return serviceProvider.GetRequiredService<CloudinaryNopHtmlHelper>();
                }
                else
                {
                    return serviceProvider.GetRequiredService<NopHtmlHelper>();
                }
            });
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => int.MaxValue;
    }
}