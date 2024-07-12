using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Payments.AamarPay.Services;

namespace NopStation.Plugin.Payments.AamarPay.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices(AamarPayPaymentDefaults.PLUGIN_SYSTEM_NAME);

        services.AddScoped<IAamarPayPaymentService, AamarPayPaymentService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => int.MaxValue;
}