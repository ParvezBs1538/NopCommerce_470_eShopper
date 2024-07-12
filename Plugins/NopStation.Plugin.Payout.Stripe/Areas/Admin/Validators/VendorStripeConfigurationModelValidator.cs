using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Payout.Stripe.Areas.Admin.Models;

namespace NopStation.Plugin.Payout.Stripe.Areas.Admin.Validators
{
    public class VendorStripeConfigurationModelValidator : BaseNopValidator<VendorStripeConfigurationModel>
    {
        public VendorStripeConfigurationModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.AccountId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.Payout.Stripe.Configuration.Fields.AccountId.Required"));
        }
    }
}
