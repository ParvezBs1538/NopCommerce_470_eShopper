using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Controllers;
using NopStation.Plugin.Misc.AmazonS3.Controllers;
using NopStation.Plugin.Misc.AmazonS3.Services;

namespace NopStation.Plugin.Misc.AmazonS3.Infrastructure
{
    internal class PluginNopStartup : INopStartup
    {
        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            services.AddTransient<PictureService>();
            services.AddTransient<AmazonS3PictureService>();

            services.AddScoped<IPictureService>(serviceProvider =>
            {
                if (serviceProvider.GetRequiredService<AmazonS3Settings>().AWSS3Enable)
                {
                    return serviceProvider.GetRequiredService<AmazonS3PictureService>();
                }
                else
                {
                    return serviceProvider.GetRequiredService<PictureService>();
                }

            });
            services.AddScoped<IAmazonS3Helper, AmazonS3Helper>();
            services.AddScoped<IRoxyFilemanAmazonS3Service, FileRoxyFilemanAmazonS3Service>();
            services.AddScoped<RoxyFilemanController, RoxyFilemanS3Controller>();
        }

        public int Order => int.MaxValue;
    }
}
