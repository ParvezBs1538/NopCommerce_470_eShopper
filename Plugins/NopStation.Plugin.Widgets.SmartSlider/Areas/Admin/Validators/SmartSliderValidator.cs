using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Validators;

public class SmartSliderValidator : BaseNopValidator<SmartSliderModel>
{
    public SmartSliderValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.Fields.Name.Required").Result);
        RuleFor(x => x.BackgroundPictureId)
            .GreaterThan(0)
            .When(x => x.ShowBackground && x.BackgroundTypeId == (int)BackgroundType.Picture).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundPicture.Required").Result);
    }
}
