using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartMegaMenu.Services;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Widgets.SmartMegaMenu");

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });

        services.AddScoped<IMegaMenuModelFactory, MegaMenuModelFactory>();
        services.AddScoped<IMegaMenuService, MegaMenuService>();

        services.AddScoped<Factories.IMegaMenuModelFactory, Factories.MegaMenuModelFactory>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 11;
}