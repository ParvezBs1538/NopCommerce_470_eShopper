﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.OrderRatings.Factories;
using NopStation.Plugin.Widgets.OrderRatings.Services;

namespace NopStation.Plugin.Widgets.OrderRatings.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.OrderRatings");

            services.AddScoped<IOrderRatingModelFactory, OrderRatingModelFactory>();
            services.AddScoped<IOrderRatingService, OrderRatingService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}