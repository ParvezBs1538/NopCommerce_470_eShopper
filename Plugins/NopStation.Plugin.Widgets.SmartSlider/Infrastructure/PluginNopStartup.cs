using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartSliders.Services;

namespace NopStation.Plugin.Widgets.SmartSliders.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Widgets.SmartSliders");

        services.AddScoped<ISmartSliderService, SmartSliderService>();
        services.AddScoped<ISmartSliderModelFactory, SmartSliderModelFactory>();

        services.AddScoped<Factories.ISmartSliderModelFactory, Factories.SmartSliderModelFactory>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 11;
}