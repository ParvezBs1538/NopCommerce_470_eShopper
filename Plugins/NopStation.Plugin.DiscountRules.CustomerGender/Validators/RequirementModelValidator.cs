using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.DiscountRules.CustomerGender.Models;

namespace NopStation.Plugin.DiscountRules.CustomerGender.Validators
{
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.CustomerGender.Fields.DiscountId.Required"));
            RuleFor(model => model.Gender)
                .NotEqual("0")
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.CustomerGender.Fields.Gender.Required"));
        }
    }
}
