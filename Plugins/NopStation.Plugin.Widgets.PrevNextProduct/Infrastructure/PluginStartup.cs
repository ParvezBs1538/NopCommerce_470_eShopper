using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.PrevNextProduct.Services;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Infrastructure;

public class PluginStartup : INopStartup
{
    public int Order => 11;

    public void Configure(IApplicationBuilder application)
    {
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Widgets.PrevNextProduct");

        services.AddScoped<IPrevNextProductService, PrevNextProductService>();
    }
}
