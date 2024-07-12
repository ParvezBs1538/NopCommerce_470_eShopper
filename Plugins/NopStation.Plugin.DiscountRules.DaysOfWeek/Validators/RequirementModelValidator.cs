using FluentValidation;
using NopStation.Plugin.DiscountRules.DaysOfWeek.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.DiscountRules.DaysOfWeek.Validators
{
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DiscountId.Required"));
            RuleFor(model => model.DaysOfWeek)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DaysOfWeek.Required"));
        }
    }
}
