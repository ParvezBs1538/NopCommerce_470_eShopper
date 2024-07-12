using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.Popups.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.Popups.Areas.Admin.Validators;

public class PopupValidator : BaseNopValidator<PopupModel>
{
    public PopupValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Popups.Popups.Fields.Name.Required").Result);
    }
}
