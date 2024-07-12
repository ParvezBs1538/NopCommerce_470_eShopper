using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Models;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Validators
{
    public class DeviceValidator : BaseNopValidator<DeviceModel>
    {
        public DeviceValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Endpoint).NotEmpty();
            RuleFor(x => x.Auth).NotEmpty();
            RuleFor(x => x.P256dh).NotEmpty();
        }
    }
}
