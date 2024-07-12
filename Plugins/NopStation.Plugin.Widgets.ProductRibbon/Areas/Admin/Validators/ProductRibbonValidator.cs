using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Validators
{
    public class ProductRibbonValidator : BaseNopValidator<ConfigurationModel>
    {
        public ProductRibbonValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ProductDetailsPageWidgetZone).NotEmpty().WithMessage(localizationService.GetResourceAsync("NopStation.ProductRibbon.Configuration.Fields.ProductDetailsPageWidgetZone.Required").Result);
            RuleFor(x => x.ProductOverviewBoxWidgetZone).NotEmpty().WithMessage(localizationService.GetResourceAsync("NopStation.ProductRibbon.Configuration.Fields.ProductOverviewBoxWidgetZone.Required").Result);
        }
    }
}
