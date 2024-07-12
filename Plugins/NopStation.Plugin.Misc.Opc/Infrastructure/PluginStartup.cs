using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.Opc.Factories;
using NopStation.Plugin.Misc.Opc.Filters;

namespace NopStation.Plugin.Misc.Opc.Infrastructure;

public class PluginStartup : INopStartup
{
    public int Order => 11;

    public void Configure(IApplicationBuilder application)
    {

    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Misc.Opc");

        services.AddScoped<IOpcModelFactory, OpcModelFactory>();

        services.AddMvc(configure =>
        {
            configure.Filters.Add<OpcActionFilter>();
        });
    }
}