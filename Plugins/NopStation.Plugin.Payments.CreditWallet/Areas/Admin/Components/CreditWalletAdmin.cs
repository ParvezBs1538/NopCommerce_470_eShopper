using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Web.Areas.Admin.Models.Customers;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Components
{
    public class CreditWalletAdminViewComponent : NopStationViewComponent
    {
        private readonly IWalletService _walletService;
        private readonly IWalletModelFactory _walletModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IActivityHistoryModelFactory _activityHistoryModelFactory;
        private readonly IInvoicePaymentModelFactory _invoicePaymentModelFactory;

        public CreditWalletAdminViewComponent(IWalletService walletService,
            IWalletModelFactory walletModelFactory,
            ICustomerService customerService,
            IActivityHistoryModelFactory activityHistoryModelFactory,
            IInvoicePaymentModelFactory invoicePaymentModelFactory)
        {
            _walletService = walletService;
            _walletModelFactory = walletModelFactory;
            _customerService = customerService;
            _activityHistoryModelFactory = activityHistoryModelFactory;
            _invoicePaymentModelFactory = invoicePaymentModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var cm = additionalData as CustomerModel;
            var wallet = await _walletService.GetWalletByCustomerIdAsync(cm.Id);
            var walletModel = wallet == null ? new WalletModel() : null;
            var customer = await _customerService.GetCustomerByIdAsync(cm.Id);

            var model = new CreditWalletModel()
            {
                WalletModel = customer == null ? null : await _walletModelFactory.PrepareWalletModelAsync(walletModel, wallet, customer),
                NewWallet = wallet is null,
                ActivityHistorySearchModel = await _activityHistoryModelFactory.PrepareActivityHistorySearchModelAsync(
                    new ActivityHistorySearchModel(), wallet),
                InvoicePaymentSearchModel = await _invoicePaymentModelFactory.PrepareInvoicePaymentSearchModelAsync(
                    new InvoicePaymentSearchModel(), wallet)
            };

            return View(model);
        }
    }
}