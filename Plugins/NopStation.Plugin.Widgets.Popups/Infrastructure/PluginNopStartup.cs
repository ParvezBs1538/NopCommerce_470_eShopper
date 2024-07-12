using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.Popups.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Popups.Services;

namespace NopStation.Plugin.Widgets.Popups.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Widgets.Popups");

        services.AddScoped<IPopupModelFactory, PopupModelFactory>();
        services.AddScoped<IPopupService, PopupService>();

        services.AddScoped<Factories.IPopupModelFactory, Factories.PopupModelFactory>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 11;
}