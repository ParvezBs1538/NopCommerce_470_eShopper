using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace NopStation.Plugin.Widgets.SmartSliders.Infrastructure;

public partial class RouteProvider : IRouteProvider
{
    #region Methods

    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var pattern = string.Empty;
        if (DataSettingsManager.IsDatabaseInstalled())
        {
            var localizationSettings = endpointRouteBuilder.ServiceProvider.GetRequiredService<LocalizationSettings>();
            if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                var langservice = endpointRouteBuilder.ServiceProvider.GetRequiredService<ILanguageService>();
                var languages = langservice.GetAllLanguagesAsync().Result.ToList();
                pattern = "{language:lang=" + languages.FirstOrDefault().UniqueSeoCode + "}/";
            }
        }

        endpointRouteBuilder.MapControllerRoute("SmartSlider", $"{pattern}load_slider_details",
            new { controller = "SmartSlider", action = "Details" });
    }

    #endregion

    #region Properties

    public int Priority => 1;

    #endregion
}
