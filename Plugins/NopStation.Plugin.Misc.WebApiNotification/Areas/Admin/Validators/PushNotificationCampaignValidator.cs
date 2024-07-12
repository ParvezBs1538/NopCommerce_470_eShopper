using System;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Validators
{
    public class WebApiNotificationCampaignValidator : BaseNopValidator<WebApiNotificationCampaignModel>
    {
        public WebApiNotificationCampaignValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Title.Required").Result);
            RuleFor(x => x.Body).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Body.Required").Result);
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Name.Required").Result);
            RuleFor(x => x.SendingWillStartOn).NotEqual(DateTime.MinValue).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.SendingWillStartOn.Required").Result);
        }
    }
}
