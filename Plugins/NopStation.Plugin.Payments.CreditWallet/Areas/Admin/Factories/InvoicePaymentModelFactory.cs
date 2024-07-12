using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Domain;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories
{
    public class InvoicePaymentModelFactory : IInvoicePaymentModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IInvoicePaymentService _invoicePaymentService;
        private readonly ICustomerService _customerService;
        private readonly IWalletService _walletService;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public InvoicePaymentModelFactory(IDateTimeHelper dateTimeHelper,
            IInvoicePaymentService invoicePaymentService,
            ICustomerService customerService,
            IWalletService walletService,
            ICurrencyService currencyService)
        {
            _dateTimeHelper = dateTimeHelper;
            _invoicePaymentService = invoicePaymentService;
            _customerService = customerService;
            _walletService = walletService;
            _currencyService = currencyService;
        }

        #endregion

        #region Methods

        public virtual Task<InvoicePaymentSearchModel> PrepareInvoicePaymentSearchModelAsync(InvoicePaymentSearchModel searchModel, Wallet wallet = null)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.SearchWalletCustomerId = wallet?.WalletCustomerId ?? 0;

            return Task.FromResult(searchModel);
        }

        public virtual async Task<InvoicePaymentListModel> PrepareInvoicePaymentListModelAsync(InvoicePaymentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var invoicePaymentList = await _invoicePaymentService.GetAllInvoicePaymentAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new InvoicePaymentListModel().PrepareToGridAsync(searchModel, invoicePaymentList, () =>
            {
                return invoicePaymentList.SelectAwait(async invoicePayment =>
                {
                    var wallet = await _walletService.GetWalletByCustomerIdAsync(invoicePayment.WalletCustomerId);
                    return await PrepareInvoicePaymentModelAsync(null, invoicePayment, wallet, true);
                });
            });

            return model;
        }

        public virtual async Task<InvoicePaymentModel> PrepareInvoicePaymentModelAsync(InvoicePaymentModel model, InvoicePayment invoicePayment,
            Wallet wallet, bool excludeProperties = false)
        {
            if (wallet == null)
                throw new ArgumentNullException(nameof(wallet));

            if (invoicePayment != null)
            {
                if (model == null)
                {
                    model = invoicePayment.ToModel<InvoicePaymentModel>();
                    model.PaymentDate = await _dateTimeHelper.ConvertToUserTimeAsync(invoicePayment.PaymentDateUtc, DateTimeKind.Utc);
                    model.PaymentDateStr = model.PaymentDate.ToShortDateString();

                    var createdBy = await _customerService.GetCustomerByIdAsync(invoicePayment.CreatedByCustomerId);
                    model.CreatedByCustomer = createdBy?.Email;
                    var updatedBy = await _customerService.GetCustomerByIdAsync(invoicePayment.UpdatedByCustomerId);
                    model.UpdatedByCustomer = updatedBy?.Email;
                }
            }

            var walletCustomer = await _customerService.GetCustomerByIdAsync(wallet.WalletCustomerId);
            model.WalletCustomerId = wallet.WalletCustomerId;
            model.WalletCustomerEmail = walletCustomer.Email;
            model.WalletCustomerName = await _customerService.GetCustomerFullNameAsync(walletCustomer);

            var currency = await _currencyService.GetCurrencyByIdAsync(wallet.CurrencyId);
            model.CurrencyCode = currency.CurrencyCode;

            if (!excludeProperties)
            {

            }

            return model;
        }

        #endregion
    }
}
