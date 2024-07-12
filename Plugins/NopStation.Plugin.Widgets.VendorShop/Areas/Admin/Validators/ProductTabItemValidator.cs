using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.ProductTabs;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Validators
{
    public class ProductTabItemValidator : BaseNopValidator<ProductTabItemModel>
    {
        public ProductTabItemValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Fields.Name.Required").Result);
        }
    }
}
