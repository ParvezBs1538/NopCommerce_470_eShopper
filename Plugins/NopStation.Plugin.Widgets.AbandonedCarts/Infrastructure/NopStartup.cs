using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.AbandonedCarts.Factories;
using NopStation.Plugin.Widgets.AbandonedCarts.Services;
using NopStation.Plugin.Widgets.AbandonedCarts.Services.Messages;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAbandonedCartService, AbandonedCartService>();
            services.AddScoped<ICustomerAbandonmentInfoService, CustomerAbandonmentInfoService>();
            services.AddScoped<IAbandonedCartMessageService, AbandonedCartMessageService>();
            services.AddScoped<IAbandonedCartMessageTokenProvider, AbandonedCartMessageTokenProvider>();

            services.AddScoped<IAbandonedCartFactory, AbandonedCartFactory>();

            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddNopStationServices("NopStation.Plugin.Widgets.AbandonedCarts", true);
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 3000;
    }
}
