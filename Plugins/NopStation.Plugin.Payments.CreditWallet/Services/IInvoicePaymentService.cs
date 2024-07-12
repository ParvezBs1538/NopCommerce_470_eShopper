using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Services
{
    public interface IInvoicePaymentService
    {
        Task InsertInvoicePaymentAsync(InvoicePayment invoicePayment);

        Task UpdateInvoicePaymentAsync(InvoicePayment invoicePayment);

        Task DeleteInvoicePaymentAsync(InvoicePayment invoicePayment);

        Task<InvoicePayment> GetInvoicePaymentByIdAsync(int invoiceId);

        Task<IList<InvoicePayment>> GetCustomerInvoicePaymentListAsync(int customerId);

        Task<IPagedList<InvoicePayment>> GetAllInvoicePaymentAsync(int customerId = 0, string invoiceReference = null,
            DateTime? paidFromUtc = null, DateTime? paidToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
