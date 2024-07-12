using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Validators;

public class SmartCarouselPictureValidator : BaseNopValidator<SmartCarouselPictureModel>
{
    public SmartCarouselPictureValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Label).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Label.Required").Result);
        RuleFor(x => x.PictureId)
            .GreaterThan(0)
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Picture.Required").Result);
    }
}
