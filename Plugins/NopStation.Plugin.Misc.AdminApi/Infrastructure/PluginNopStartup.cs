using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;

namespace NopStation.Plugin.Misc.AdminApi.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(option =>
        {
            option.AddPolicy("AllowAll", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        services.AddScoped<IAdminApiModelFactory, AdminApiModelFactory>();
        services.AddScoped<IAdminApiSiteMapModelFactory, AdminApiSiteMapModelFactory>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseCors("AllowAll");
        app.UseMiddleware<JwtAuthMiddleware>();
    }

    public int Order => 1;
}