﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.CategoryBanners.Services;

namespace NopStation.Plugin.Widgets.CategoryBanners.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.CategoryBanners");

            services.AddScoped<ICategoryBannerService, CategoryBannerService>();

            services.AddScoped<ICategoryBannerModelFactory, CategoryBannerModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}