﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Tax.TaxJar.Services;

namespace NopStation.Plugin.Tax.TaxJar.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<TaxJarManager>();
            services.AddScoped<ITaxJarCategoryService, TaxJarCategoryService>();
            services.AddScoped<ITaxJarService, TaxJarService>();
            services.AddScoped<TaxjarTransactionLogService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 1;
    }
}