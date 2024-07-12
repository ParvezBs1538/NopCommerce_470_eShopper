using System.Threading.Tasks;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories
{
    public interface IInvoicePaymentModelFactory
    {
        Task<InvoicePaymentSearchModel> PrepareInvoicePaymentSearchModelAsync(InvoicePaymentSearchModel searchModel, Wallet wallet = null);

        Task<InvoicePaymentListModel> PrepareInvoicePaymentListModelAsync(InvoicePaymentSearchModel searchModel);

        Task<InvoicePaymentModel> PrepareInvoicePaymentModelAsync(InvoicePaymentModel model, InvoicePayment invoicePayment,
            Wallet wallet, bool excludeProperties = false);
    }
}
