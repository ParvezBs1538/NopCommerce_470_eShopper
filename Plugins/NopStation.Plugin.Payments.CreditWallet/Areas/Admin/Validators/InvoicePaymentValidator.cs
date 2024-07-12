using System;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Validators
{
    public class InvoicePaymentValidator : BaseNopValidator<InvoicePaymentModel>
    {
        public InvoicePaymentValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.InvoiceReference).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.Fields.InvoiceReference.Required"));
            RuleFor(x => x.PaymentDate).NotEqual(DateTime.MinValue).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.Fields.PaymentDate.Required"));
        }
    }
}