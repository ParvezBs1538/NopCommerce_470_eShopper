using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Extensions;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Misc.WebApi");

            services.AddCors(option =>
            {
                option.AddPolicy("AllowAll", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            services.AddScoped<IWebApiModelFactory, WebApiModelFactory>();
            services.AddScoped<ISliderModelFactory, SliderModelFactory>();
            services.AddScoped<IDeviceModelFactory, DeviceModelFactory>();
            services.AddScoped<ICategoryIconModelFactory, CategoryIconModelFactory>();

            services.AddScoped<ICategoryIconService, CategoryIconService>();
            services.AddScoped<IApiDeviceService, ApiDeviceService>();
            services.AddScoped<IApiSliderService, ApiSliderService>();
            services.AddScoped<ICustomerApiService, CustomerApiService>();
            services.AddScoped<IApiStringResourceService, ApiStringResourceService>();
            services.AddScoped<IProductAttributeApiParser, ProductAttributeApiParser>();
            services.AddScoped<Factories.ICommonApiModelFactory, Factories.CommonApiModelFactory>();
            services.AddScoped<Factories.ISliderModelFactory, Factories.SliderModelFactory>();
            services.AddScoped<Factories.ICatalogApiModelFactory, Factories.CatalogApiModelFactory>();
            services.AddScoped<IProductApiService, ProductApiService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseApiExceptionHandler();
            app.UseApiNotFound();
            app.UseCors("AllowAll");
            app.UseMiddleware<JwtAuthMiddleware>();
        }

        public int Order => 1;
    }
}