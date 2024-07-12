using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.CreditWallet.Models;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Controllers
{
    public class CreditWalletController : NopStationPublicController
    {
        private readonly IWorkContext _workContext;
        private readonly IWalletService _walletService;
        private readonly ICustomerService _customerService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly CreditWalletSettings _creditWalletSettings;
        private readonly IInvoicePaymentService _invoicePaymentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;

        public CreditWalletController(IWorkContext workContext,
            IWalletService walletService,
            ICustomerService customerService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            CreditWalletSettings creditWalletSettings,
            IInvoicePaymentService invoicePaymentService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService)
        {
            _workContext = workContext;
            _walletService = walletService;
            _customerService = customerService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _creditWalletSettings = creditWalletSettings;
            _invoicePaymentService = invoicePaymentService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
        }

        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var wallet = await _walletService.GetWalletByCustomerIdAsync(customer.Id);
            if (wallet == null)
                return RedirectToRoute("Homepage");

            var currency = await _currencyService.GetCurrencyByIdAsync(wallet.CurrencyId);

            var model = new WalletDetailsModel()
            {
                Active = wallet.Active,
                AllowOverspend = wallet.AllowOverspend,
                AvailableCreditValue = wallet.AvailableCredit,
                CreditLimit = wallet.CreditLimit,
                CreditUsedValue = wallet.CreditUsed,
                Id = customer.Id,
                LowCredit = wallet.AvailableCredit < wallet.WarnUserForCreditBelow,
                AvailableCredit = await _priceFormatter.FormatPriceAsync(wallet.AvailableCredit, true, currency),
                CreditUsed = await _priceFormatter.FormatPriceAsync(wallet.CreditUsed, true, currency),
                ShowInvoicesInCustomerWalletPage = _creditWalletSettings.ShowInvoicesInCustomerWalletPage
            };

            if (_creditWalletSettings.ShowInvoicesInCustomerWalletPage)
            {
                var invoices = await _invoicePaymentService.GetAllInvoicePaymentAsync(customer.Id,
                    pageSize: _creditWalletSettings.MaxInvoicesToShowInCustomerWalletPage);

                foreach (var invoice in invoices)
                {
                    model.Invoices.Add(new WalletDetailsModel.InvoicePaymentModel
                    {
                        AmountValue = invoice.Amount,
                        Id = invoice.Id,
                        InvoiceReference = invoice.InvoiceReference,
                        PaymentDate = await _dateTimeHelper.ConvertToUserTimeAsync(invoice.PaymentDateUtc, DateTimeKind.Utc),
                        Amount = await _priceFormatter.FormatPriceAsync(invoice.Amount, true, currency),
                        Note = invoice.Note
                    });
                }
            }

            return View(model);
        }
    }
}
