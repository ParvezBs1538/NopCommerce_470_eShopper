using FluentValidation;
using NopStation.Plugin.DiscountRules.TimeOfDay.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.DiscountRules.TimeOfDay.Validators
{
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.TimeOfDay.Fields.DiscountId.Required"));
        }
    }
}
