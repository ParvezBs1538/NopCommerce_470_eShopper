using System;
using FluentValidation;
using NopStation.Plugin.Payments.Quickstream.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Payments.Quickstream.Validators
{
    public partial class CreditCardPaymentInfoValidator : BaseNopValidator<CreditCardPaymentModel>
    {
        public CreditCardPaymentInfoValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.CardholderName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.QuickStream.CreditCardPayments.Fields.CardHolderName.Required"));
            RuleFor(x => x.CardNumber).IsCreditCard().WithMessageAwait(localizationService.GetResourceAsync("NopStation.QuickStream.CreditCardPayments.Fields.CardNumber.NotValid"));
            RuleFor(x => x.CardCode).Matches(@"^[0-9]{3,4}$").WithMessageAwait(localizationService.GetResourceAsync("NopStation.QuickStream.CreditCardPayments.Fields.CVN.NotValid"));
            RuleFor(x => x.ExpireMonth).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.QuickStream.CreditCardPayments.Fields.ExpireMonth.Required"));
            RuleFor(x => x.ExpireYear).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.QuickStream.CreditCardPayments.Fields.ExpireYear.Required"));
            RuleFor(x => x.ExpireMonth).Must((x, context) =>
            {
                //not specified yet
                if (string.IsNullOrEmpty(x.ExpireYear) || string.IsNullOrEmpty(x.ExpireMonth))
                    return true;

                //the cards remain valid until the last calendar day of that month
                //If, for example, an expiration date reads 06/15, this means it can be used until midnight on June 30, 2015
                var enteredDate = new DateTime(int.Parse(x.ExpireYear), int.Parse(x.ExpireMonth), 1).AddMonths(1);

                if (enteredDate < DateTime.Now)
                    return false;

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("NopStation.QuickStream.CreditCardPayments.Fields.ExpirationDate.Expired"));
        }
    }
}