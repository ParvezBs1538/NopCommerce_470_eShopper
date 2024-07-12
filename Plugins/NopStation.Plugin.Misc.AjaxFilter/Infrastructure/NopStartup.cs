using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Factories;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services;
using NopStation.Plugin.Misc.AjaxFilter.Services;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.AjaxFilter.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public int Order => 10000;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //add custom view location engine
            services.AddNopStationServices("NopStation.Plugin.Misc.AjaxFilter");

            services.AddScoped<IAjaxFilterBaseAdminModelFactory, AjaxFilterBaseAdminModelFactory>();
            services.AddScoped<IAjaxFilterModelFactory, AjaxFilterModelFactory>();
            services.AddScoped<IAjaxFilterSpecificationAttributeModelFactory, AjaxFilterSpecificationAttributeModelFactory>();
            services.AddScoped<IAjaxFilterSpecificationAttributeService, AjaxFilterSpecificationAttributeService>();
            services.AddScoped<IAjaxFilterParentCategoryService, AjaxFilterParentCategoryService>();

            services.AddScoped<IAjaxFilterService, AjaxFilterService>();
            services.AddScoped<ICatalogModelFactory, Factories.AjaxFilterCatalogModelFactory>();
            services.AddScoped<IProductModelFactory, Factories.AjaxFilterProductModelFactory>();
            services.AddScoped<Factories.IAjaxSearchModelFactory, Factories.AjaxSearchModelFactory>();
            services.AddScoped<Factories.ICategoryImageFactory, Factories.CategoryImageFactory>();
        }
    }
}
