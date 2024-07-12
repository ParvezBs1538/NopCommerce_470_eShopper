using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Validators
{
    public class SliderItemValidator : BaseNopValidator<SliderItemModel>
    {
        public SliderItemValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.PictureId).GreaterThan(0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Picture.Required").Result);
            RuleFor(x => x.MobilePictureId).GreaterThan(0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.MobilePicture.Required").Result);
        }
    }
}
