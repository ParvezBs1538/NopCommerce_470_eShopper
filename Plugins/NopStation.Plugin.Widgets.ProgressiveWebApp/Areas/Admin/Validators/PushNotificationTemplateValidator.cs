using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Validators
{
    public class PushNotificationTemplateValidator : BaseNopValidator<PushNotificationTemplateModel>
    {
        public PushNotificationTemplateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Title.Required").Result);
            RuleFor(x => x.IconId)
                .GreaterThan(0)
                .When(x => !x.UseDefaultIcon)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PWA.PushNotificationTemplates.Fields.IconId.Required").Result);
        }
    }
}
