using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Validators;

public class SmartSliderItemValidator : BaseNopValidator<SmartSliderItemModel>
{
    public SmartSliderItemValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.DesktopPictureId)
           .GreaterThan(0)
           .When(x => x.ContentTypeId == (int)ContentType.Picture).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.Items.Fields.DesktopPictureId.Required").Result);
        RuleFor(x => x.MobilePictureId)
            .GreaterThan(0)
            .When(x => x.ContentTypeId == (int)ContentType.Picture).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.Items.Fields.MobilePictureId.Required").Result);
        RuleFor(x => x.EmbeddedLink)
            .NotNull()
            .When(x => x.ContentTypeId == (int)ContentType.EmbeddedLink).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.Items.Fields.EmbeddedLink.Required").Result);
        RuleFor(x => x.Text)
            .NotNull()
            .When(x => x.ContentTypeId == (int)ContentType.Text).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Text.Required").Result);

    }
}
