using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Factories;
using NopStation.Plugin.Misc.SqlManager.Services;

namespace NopStation.Plugin.Misc.SqlManager.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Misc.SqlManager", excludepublicView: true);

            services.AddScoped<ISqlParameterService, SqlParameterService>();
            services.AddScoped<ISqlReportService, SqlReportService>();

            services.AddScoped<ISqlReportModelFactory, SqlReportModelFactory>();
            services.AddScoped<ISqlParameterModelFactory, SqlParameterModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}