using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Validators;

public class BadgeValidator : BaseNopValidator<BadgeModel>
{
    public BadgeValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Badges.Fields.Name.Required").Result);
        RuleFor(x => x.Text)
            .NotEmpty()
            .When(x => x.ContentTypeId == (int)ContentType.Text && x.BadgeTypeId == (int)BadgeType.CustomProducts)
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Badges.Fields.Text.Required").Result);
    }
}