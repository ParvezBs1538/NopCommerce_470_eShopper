using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.MegaMenu.Factories;
using NopStation.Plugin.Widgets.MegaMenu.Services;

namespace NopStation.Plugin.Widgets.MegaMenu.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });
        services.AddScoped<ICategoryIconService, CategoryIconService>();
        services.AddScoped<ICategoryIconModelFactory, CategoryIconModelFactory>();
        services.AddScoped<IMegaMenuModelFactory, MegaMenuModelFactory>();
        services.AddScoped<IMegaMenuCoreService, MegaMenuCoreService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 110;
}