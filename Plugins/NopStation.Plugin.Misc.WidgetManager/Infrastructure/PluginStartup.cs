using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WidgetManager.Services;

namespace NopStation.Plugin.Misc.WidgetManager.Infrastructure;

public class PluginStartup : INopStartup
{
    public int Order => 11;

    public void Configure(IApplicationBuilder application)
    {
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Misc.WidgetManager", false, true);

        services.AddScoped<IWidgetZoneService, WidgetZoneService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IConditionService, ConditionService>();

        services.AddScoped<IWidgetZoneModelFactory, WidgetZoneModelFactory>();
        services.AddScoped<IScheduleModelFactory, ScheduleModelFactory>();
        services.AddScoped<IConditionModelFactory, ConditionModelFactory>();
    }
}
