using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.VendorShop.Factories;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.VendorShop");
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new CustomViewEngine());
            });

            services.AddScoped<IOCarouselService, OCarouselService>();
            services.AddScoped<ISliderService, SliderService>();
            services.AddScoped<IOCarouselModelFactory, OCarouselModelFactory>();
            services.AddScoped<ISliderModelFactory, SliderModelFactory>();

            services.AddScoped<Areas.Admin.Factories.IOCarouselModelFactory, Areas.Admin.Factories.OCarouselModelFactory>();
            services.AddScoped<Areas.Admin.Factories.ISliderModelFactory, Areas.Admin.Factories.SliderModelFactory>();

            services.AddScoped<Areas.Admin.Factories.IVendorProfileModelFactory, Areas.Admin.Factories.VendorProfileModelFactory>();
            services.AddScoped<IVendorProfileService, VendorProfileService>();

            services.AddScoped<IProductTabService, ProductTabService>();
            services.AddScoped<IProductTabModelFactory, ProductTabModelFactory>();
            services.AddScoped<Areas.Admin.Factories.IProductTabModelFactory, Areas.Admin.Factories.ProductTabModelFactory>();


            services.AddScoped<IProductReviewModelFactory, ProductReviewModelFactory>();
            services.AddScoped<IVendorCategoryService, VendorCategoryService>();
            services.AddScoped<IVendorSubscriberService, VendorSubscriberService>();
            services.AddScoped<Areas.Admin.Factories.IVendorSubscriberModelFactory, Areas.Admin.Factories.VendorSubscriberModelFactory>();
            services.AddScoped<IVendorShopNotificationService, VendorShopNotificationService>();
            services.AddScoped<IVendorCatalogModelFactory, VendorCatalogModelFactory>();
            services.AddScoped<IVendorShopFeatureService, VendorShopFeatureService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}