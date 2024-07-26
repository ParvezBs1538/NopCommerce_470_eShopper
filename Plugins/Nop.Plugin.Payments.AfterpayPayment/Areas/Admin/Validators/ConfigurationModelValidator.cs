using FluentValidation;
using Nop.Plugin.Payments.AfterpayPayment.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.AfterpayPayment.Areas.Admin.Validators
{
    public class ConfigurationModelValidator : BaseNopValidator<ConfigurationModel>
    {
        public ConfigurationModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.Afterpay.ConfigurationModel.Fields.MerchantId.Required"));
            RuleFor(x => x.MerchantKey)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.Afterpay.ConfigurationModel.Fields.MerchantKey.Required"));
        }
    }
}
