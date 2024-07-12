using FluentValidation;
using NopStation.Plugin.DiscountRules.PaymentMethod.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.DiscountRules.PaymentMethod.Validators
{
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.PaymentMethod.Fields.DiscountId.Required"));
            RuleFor(model => model.PaymentMethod)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.PaymentMethod.Fields.PaymentMethod.Required"));
        }
    }
}
