using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Payments.Quickstream.Areas.Admin.Factories;
using NopStation.Plugin.Payments.Quickstream.Services;

namespace NopStation.Plugin.Payments.Quickstream.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Payments.Quickstream");

            services.AddScoped<IAcceptedCardModelFactory, AcceptedCardModelFactory>();

            services.AddScoped<IQuickStreamPaymentService, QuickStreamPaymentService>();
            services.AddScoped<IAcceptedCardService, AcceptedCardService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => int.MaxValue;
    }
}
