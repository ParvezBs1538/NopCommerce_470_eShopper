using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Payments.Stripe.Models;

namespace NopStation.Plugin.Payments.Stripe.Validators
{
    public class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
    {
        public PaymentInfoValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.CardholderName).NotEmpty().WithMessage(localizationService.GetResourceAsync("Payment.CardholderName.Required").Result);
            RuleFor(x => x.CardNumber).IsCreditCard().WithMessage(localizationService.GetResourceAsync("Payment.CardNumber.Wrong").Result);
            RuleFor(x => x.CardCode).Matches(@"^[0-9]{3,4}$").WithMessage(localizationService.GetResourceAsync("Payment.CardCode.Wrong").Result);
            RuleFor(x => x.ExpireMonth).NotEmpty().WithMessage(localizationService.GetResourceAsync("Payment.ExpireMonth.Required").Result);
            RuleFor(x => x.ExpireYear).NotEmpty().WithMessage(localizationService.GetResourceAsync("Payment.ExpireYear.Required").Result);
        }
    }
}
