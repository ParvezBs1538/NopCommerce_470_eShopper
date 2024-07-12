using System;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Validators
{
    public class SmartGroupNotificationValidator : BaseNopValidator<GroupNotificationModel>
    {
        public SmartGroupNotificationValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Fields.Title.Required").Result);
            RuleFor(x => x.Body).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Fields.Body.Required").Result);
            RuleFor(x => x.IconId)
                .GreaterThan(0)
                .When(x => !x.UseDefaultIcon)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Fields.IconId.Required").Result);
            RuleFor(x => x.SmartGroupId)
                .NotNull()
                .When(x => !x.SendToAll)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Fields.SmartGroup.Required").Result);
            RuleFor(x => x.SmartGroupId)
                .GreaterThan(0)
                .When(x => !x.SendToAll)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Fields.SmartGroup.Required").Result);
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Fields.Name.Required").Result);
            RuleFor(x => x.SendingWillStartOn).NotEqual(DateTime.MinValue).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Fields.SendingWillStartOn.Required").Result);
        }
    }
}
