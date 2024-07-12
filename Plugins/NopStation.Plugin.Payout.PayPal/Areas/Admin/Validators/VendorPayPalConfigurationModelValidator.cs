using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Payout.PayPal.Areas.Admin.Models;

namespace NopStation.Plugin.Payout.PayPal.Areas.Admin.Validators
{
    public class VendorPayPalConfigurationModelValidator : BaseNopValidator<VendorPayPalConfigurationModel>
    {
        public VendorPayPalConfigurationModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.PayPalEmail)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.Payout.PayPal.Configuration.Fields.PayPalEmail.Required"));
        }
    }
}
