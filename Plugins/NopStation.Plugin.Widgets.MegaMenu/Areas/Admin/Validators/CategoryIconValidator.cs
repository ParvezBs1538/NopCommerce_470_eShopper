using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Validators;

public class CategoryIconValidator : BaseNopValidator<CategoryIconModel>
{
    public CategoryIconValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.PictureId)
            .GreaterThan(0)
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Picture.Required").Result);
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Category.Required").Result);
    }
}
