using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.AbandonedCarts.Factories;
using NopStation.Plugin.Widgets.Product360View.Services;

namespace NopStation.Plugin.Widgets.Product360View.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IProductImageSettingService, ProductImageSettingService>();
            services.AddScoped<IProductPictureMappingService, ProductPictureMappingService>();

            services.AddScoped<IProduct360ModelFactory, Product360ModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 3000;
    }
}
