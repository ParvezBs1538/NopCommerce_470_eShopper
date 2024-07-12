using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Validators
{
    public class FAQItemValidator : BaseNopValidator<FAQItemModel>
    {
        public FAQItemValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Question).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQItem.Fields.Question.Required").Result);
            RuleFor(x => x.Answer).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQItem.Fields.Answer.Required").Result);
        }
    }
}
