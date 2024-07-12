using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartCarousels.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Widgets.SmartCarousels");

        services.AddScoped<ISmartCarouselService, SmartCarouselService>();
        services.AddScoped<ISmartCarouselModelFactory, SmartCarouselModelFactory>();

        services.AddScoped<Factories.ISmartCarouselModelFactory, Factories.SmartCarouselModelFactory>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 11;
}