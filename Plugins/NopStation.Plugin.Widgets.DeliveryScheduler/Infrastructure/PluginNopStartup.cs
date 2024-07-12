using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DeliveryScheduler.Filters;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.DeliveryScheduler");

            services.AddMvc(configure =>
            {
                var filters = configure.Filters;
                filters.Add<DeliverySlotActionFilter>();
            });

            services.AddScoped<IDeliverySlotService, DeliverySlotService>();
            services.AddScoped<IDeliveryCapacityService, DeliveryCapacityService>();
            services.AddScoped<ISpecialDeliveryCapacityService, SpecialDeliveryCapacityService>();
            services.AddScoped<ISpecialDeliveryOffsetService, SpecialDeliveryOffsetService>();
            services.AddScoped<ICustomShippingService, CustomShippingService>();
            services.AddScoped<IOrderDeliverySlotService, OrderDeliverySlotService>();

            services.AddScoped<Factories.IDeliverySchedulerModelFactory, Factories.DeliverySchedulerModelFactory>();

            services.AddScoped<IDeliverySlotModelFactory, DeliverySlotModelFactory>();
            services.AddScoped<IDeliveryCapacityModelFactory, DeliveryCapacityModelFactory>();
            services.AddScoped<IDeliverySchedulerModelFactory, DeliverySchedulerModelFactory>();
            services.AddScoped<ISpecialDeliveryCapacityModelFactory, SpecialDeliveryCapacityModelFactory>();
            services.AddScoped<IOrderDeliverySlotModelFactory, OrderDeliverySlotModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}