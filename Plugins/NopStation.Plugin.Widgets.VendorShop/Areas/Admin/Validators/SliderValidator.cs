using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Validators
{
    public class SliderValidator : BaseNopValidator<SliderModel>
    {
        public SliderValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Name.Required").Result);
            RuleFor(x => x.VendorId)
                .GreaterThan(0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.VendorId.Required").Result);
            RuleFor(x => x.BackgroundPictureId)
                .GreaterThan(0)
                .When(x => x.ShowBackgroundPicture)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.BackGroundPicture.Required").Result);
        }
    }
}
