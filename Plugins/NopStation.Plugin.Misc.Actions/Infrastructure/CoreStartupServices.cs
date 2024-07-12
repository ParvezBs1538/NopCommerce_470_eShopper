using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Nop.Web.Framework.Infrastructure.Extensions;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public static class CoreStartupServices
{
    public static void AddNopStationServices(this IServiceCollection services, string folderName, bool rootAdmin = false, bool excludepublicView = false)
    {
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander(folderName, rootAdmin, excludepublicView));
        });

        services.AddMvc(configure =>
        {
            var filters = configure.Filters;
            filters.Add<CoreActionFilter>();
        });

        services.AddHttpClient<NopStationHttpClient>().WithProxy();

        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<ISmsPluginManager, SmsPluginManager>();
        services.AddScoped<INopStationPluginManager, NopStationPluginManager>();
        services.AddScoped<IProductAttributeParserApi, ProductAttributeParserApi>();
    }
}