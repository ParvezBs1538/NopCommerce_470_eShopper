using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Factories;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Factories;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.PickupInStoreAdvance");

            services.AddScoped<IStorePickupPointService, StorePickupPointService>();
            services.AddScoped<IStorePickupPointModelFactory, StorePickupPointModelFactory>();
            services.AddScoped<IPickupInStoreDeliveryManageService, PickupInStoreDeliveryManageService>();
            services.AddScoped<IPickupInStoreDeliveryManageModelFactory, PickupInStoreDeliveryManageModelFactory>();
            services.AddScoped<IPickupInStoreMessageService, PickupInStoreMessageService>();
            services.AddScoped<IPickupPointExportImportService, PickupPointExportImportService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 100;
    }
}