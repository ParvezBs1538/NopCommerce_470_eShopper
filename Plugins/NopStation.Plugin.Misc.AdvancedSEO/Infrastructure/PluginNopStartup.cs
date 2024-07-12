using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AdvancedSEO.Helpers;
using NopStation.Plugin.Misc.AdvancedSEO.Services;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.AdvancedSEO.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public int Order => 11;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices(AdvancedSEOPluginDefaults.PluginOutputDir);

            services.AddScoped<ISEOTokenProvider, SEOTokenProvider>();

            #region Common Service

            services.AddScoped<ICategorySEOTemplateService, CategorySEOTemplateService>();
            services.AddScoped<ICategoryCategorySEOTemplateMappingService, CategoryCategorySEOTemplateMappingService>();
            services.AddScoped<IManufacturerManufacturerSEOTemplateMappingService, ManufacturerManufacturerSEOTemplateMappingService>();
            services.AddScoped<IManufacturerSEOTemplateService, ManufacturerSEOTemplateService>();
            services.AddScoped<IProductSEOTemplateService, ProductSEOTemplateService>();
            services.AddScoped<IProductProductSEOTemplateMappingService, ProductProductSEOTemplateMappingService>();


            #endregion

            #region Admin Factories

            services.AddScoped<ICategorySEOTemplateModelFactory, CategorySEOTemplateModelFactory>();
            services.AddScoped<ICategoryCategorySEOTemplateMappingModelFactory, CategoryCategorySEOTemplateMappingModelFactory>();
            services.AddScoped<IManufacturerSEOTemplateModelFactory, ManufacturerSEOTemplateModelFactory>();
            services.AddScoped<IManufacturerManufacturerSEOTemplateMappingModelFactory, ManufacturerManufacturerSEOTemplateMappingModelFactory>();
            services.AddScoped<IProductSEOTemplateModelFactory, ProductSEOTemplateModelFactory>();
            services.AddScoped<IProductProductSEOTemplateMappingModelFactory, ProductProductSEOTemplateMappingModelFactory>();

            #endregion
        }
    }
}
