using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Validators
{
    public class WebApiNotificationTemplateValidator : BaseNopValidator<WebApiNotificationTemplateModel>
    {
        public WebApiNotificationTemplateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Title.Required").Result);
            RuleFor(x => x.Name)
                .NotEmpty()
                .When(x => x.Id == 0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Name.Required").Result);
            RuleFor(x => x.DelayBeforeSend)
                .NotNull()
                .When(x => !x.SendImmediately)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.DelayBeforeSend.Required").Result);
            RuleFor(x => x.DelayBeforeSend)
                .GreaterThan(0)
                .When(x => !x.SendImmediately)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.DelayBeforeSend.GreaterThanZero").Result);
        }
    }
}
