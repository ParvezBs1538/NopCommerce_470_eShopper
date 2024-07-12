using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Validators
{
    public class ProductTabItemValidator : BaseNopValidator<ProductTabItemModel>
    {
        public ProductTabItemValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabItems.Fields.Name.Required").Result);
        }
    }
}
