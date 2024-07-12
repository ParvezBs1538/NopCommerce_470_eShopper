using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProductBadge.Services;

namespace NopStation.Plugin.Widgets.ProductBadge.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Widgets.ProductBadge");

        services.AddScoped<IBadgeService, BadgeService>();
        services.AddScoped<IBadgeModelFactory, BadgeModelFactory>();

        services.AddScoped<Factories.IBadgeModelFactory, Factories.BadgeModelFactory>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 11;
}