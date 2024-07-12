using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;
using NopStation.Plugin.Misc.QuoteCart.Factories;
using NopStation.Plugin.Misc.QuoteCart.Services;
using NopStation.Plugin.Misc.QuoteCart.Services.Email;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;
using NopStation.Plugin.Misc.QuoteCart.Services.RequestMessage;

namespace NopStation.Plugin.Misc.QuoteCart.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IQuoteCartService, QuoteCartService>();
        services.AddScoped<IQuoteCartModelFactory, QuoteCartModelFactory>();
        services.AddScoped<IQuoteFormModelFactory, QuoteFormModelFactory>();
        services.AddScoped<IQuoteRequestService, QuoteRequestService>();
        services.AddScoped<IQuoteRequestModelFactory, QuoteRequestModelFactory>();
        services.AddScoped<IRequestProcessingService, RequestProcessingService>();
        services.AddScoped<IQuoteRequestItemService, QuoteRequestItemService>();
        services.AddScoped<IQuoteCartEmailService, QuoteCartEmailService>();
        services.AddScoped<IPublicQuoteRequestModelFactory, PublicQuoteRequestModelFactory>();
        services.AddScoped<IFormAttributeModelFactory, FormAttributeModelFactory>();
        services.AddScoped<IQuoteRequestMessageService, QuoteRequestMessageService>();
        services.AddScoped<IQuoteRequestWhitelistService, QuoteRequestWhitelistService>();
        services.AddScoped<IQuoteWhitelistModelFactory, QuoteWhitelistModelFactory>();
        services.AddScoped<IQuoteFormService, QuoteFormService>();
        services.AddScoped<IFormAttributeService, FormAttributeService>();
        services.AddScoped<IFormAttributeParser, FormAttributeParser>();
        services.AddScoped<IFormAttributeFormatter, FormAttributeFormatter>();

        services.AddNopStationServices(QuoteCartDefaults.SYSTEM_NAME);
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}