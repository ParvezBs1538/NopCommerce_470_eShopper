using FluentValidation;
using NopStation.Plugin.DiscountRules.AffiliatedCustomers.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.DiscountRules.AffiliatedCustomers.Validators
{
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.AffiliatedCustomers.Fields.DiscountId.Required"));
            RuleFor(model => model.AffiliateId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.AffiliatedCustomers.Fields.Affiliate.Required"));
        }
    }
}
