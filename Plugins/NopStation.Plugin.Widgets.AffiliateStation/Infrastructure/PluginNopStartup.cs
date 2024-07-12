using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.AffiliateStation.Services;

namespace NopStation.Plugin.Widgets.AffiliateStation.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public int Order => 1001;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.AffiliateStation");

            services.AddScoped<IAffiliateCustomerService, AffiliateCustomerService>();
            services.AddScoped<ICatalogCommissionService, CatalogCommissionService>();
            services.AddScoped<IOrderCommissionService, OrderCommissionService>();

            services.AddScoped<Factories.IAffiliateCustomerModelFactory, Factories.AffiliateCustomerModelFactory>();
            services.AddScoped<Factories.IOrderCommissionModelFactory, Factories.OrderCommissionModelFactory>();

            services.AddScoped<IAffiliateCustomerModelFactory, AffiliateCustomerModelFactory>();
            services.AddScoped<ICatalogCommissionModelFactory, CatalogCommissionModelFactory>();
            services.AddScoped<IAffiliateStationModelFactory, AffiliateStationModelFactory>();
            services.AddScoped<IOrderCommissionModelFactory, OrderCommissionModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }
    }
}
