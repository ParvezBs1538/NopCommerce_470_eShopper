using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Validators
{
    public class OCarouselValidator : BaseNopValidator<OCarouselModel>
    {
        public OCarouselValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Name.Required").Result);
            RuleFor(x => x.Title)
                .NotEmpty()
                .When(x => x.DisplayTitle)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Title.Required").Result);
            RuleFor(x => x.VendorId)
                .GreaterThan(0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.VendorId.Required").Result);
            RuleFor(x => x.NumberOfItemsToShow)
                .GreaterThan(0)
                .When(x => x.DataSourceTypeId != (int)DataSourceTypeEnum.CustomProducts)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Required").Result);
        }
    }
}
