using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Validators
{
    public class FAQCategoryValidator : BaseNopValidator<FAQCategoryModel>
    {
        public FAQCategoryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQCategories.Fields.Name.Required").Result);
            RuleFor(x => x.Description).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQCategories.Fields.Description.Required").Result);
        }
    }
}
