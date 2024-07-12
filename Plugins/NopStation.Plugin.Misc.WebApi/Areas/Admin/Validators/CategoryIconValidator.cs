using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Validators
{
    public class CategoryIconValidator : BaseNopValidator<CategoryIconModel>
    {
        public CategoryIconValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApi.CategoryIcons.Fields.Category.Required").Result);
        }
    }
}