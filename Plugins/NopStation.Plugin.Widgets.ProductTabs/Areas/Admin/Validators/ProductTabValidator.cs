using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Validators
{
    public class ProductTabValidator : BaseNopValidator<ProductTabModel>
    {
        public ProductTabValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabs.Fields.Name.Required").Result);
            RuleFor(x => x.TabTitle).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabs.Fields.Title.Required").Result);
            RuleFor(x => x.PictureId).GreaterThan(0).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabs.Fields.Picture.Required").Result);
        }
    }
}
