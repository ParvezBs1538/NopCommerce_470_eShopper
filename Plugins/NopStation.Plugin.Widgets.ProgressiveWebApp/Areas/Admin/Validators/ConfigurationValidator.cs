using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Validators
{
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.VapidSubjectEmail).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PWA.Configuration.Fields.VapidSubjectEmail.Required").Result);
            RuleFor(x => x.VapidPrivateKey).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PWA.Configuration.Fields.VapidPrivateKey.Required").Result);
            RuleFor(x => x.VapidPublicKey).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PWA.Configuration.Fields.VapidPublicKey.Required").Result);
            RuleFor(x => x.ManifestShortName)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.ManifestName))
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PWA.Configuration.Fields.ManifestNameOrShortName.Required").Result);
        }
    }
}
