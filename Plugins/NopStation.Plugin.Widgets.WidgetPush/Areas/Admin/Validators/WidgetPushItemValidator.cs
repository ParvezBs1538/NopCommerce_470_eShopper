using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Validators
{
    public class WidgetPushItemValidator : BaseNopValidator<WidgetPushItemModel>
    {
        public WidgetPushItemValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Name.Required").Result);
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Content.Required").Result);
            RuleFor(x => x.WidgetZone)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.WidgetZone.Required").Result);
        }
    }
}
