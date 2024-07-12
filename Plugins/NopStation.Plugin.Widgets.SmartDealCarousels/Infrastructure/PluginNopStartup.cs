using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartDealCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Widgets.SmartDealCarousels");

        services.AddScoped<ISmartDealCarouselService, SmartDealCarouselService>();
        services.AddScoped<ISmartDealCarouselModelFactory, SmartDealCarouselModelFactory>();

        services.AddScoped<Factories.ISmartDealCarouselModelFactory, Factories.SmartDealCarouselModelFactory>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 11;
}