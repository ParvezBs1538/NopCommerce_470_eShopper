using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Infrastructure
{
    public class NopPluginStartup : INopStartup
    {
        public int Order => 1000;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Payments.CreditWallet");

            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IActivityHistoryService, ActivityHistoryService>();
            services.AddScoped<IInvoicePaymentService, InvoicePaymentService>();

            services.AddScoped<IInvoicePaymentModelFactory, InvoicePaymentModelFactory>();
            services.AddScoped<IWalletModelFactory, WalletModelFactory>();
            services.AddScoped<IActivityHistoryModelFactory, ActivityHistoryModelFactory>();
        }
    }
}
