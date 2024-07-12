using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Validators
{
    public class SmartGroupValidator : BaseNopValidator<SmartGroupModel>
    {
        public SmartGroupValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PushNop.SmartGroups.Fields.Name.Required").Result);
        }
    }
}
