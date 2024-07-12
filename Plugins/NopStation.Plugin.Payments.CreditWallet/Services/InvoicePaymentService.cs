using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Services
{
    public class InvoicePaymentService : IInvoicePaymentService
    {
        private readonly IRepository<InvoicePayment> _invoicePaymentRepository;

        public InvoicePaymentService(IRepository<InvoicePayment> invoicePaymentRepository)
        {
            _invoicePaymentRepository = invoicePaymentRepository;
        }

        public async Task DeleteInvoicePaymentAsync(InvoicePayment invoicePayment)
        {
            await _invoicePaymentRepository.DeleteAsync(invoicePayment);
        }

        public async Task InsertInvoicePaymentAsync(InvoicePayment invoicePayment)
        {
            await _invoicePaymentRepository.InsertAsync(invoicePayment);
        }

        public async Task UpdateInvoicePaymentAsync(InvoicePayment invoicePayment)
        {
            await _invoicePaymentRepository.UpdateAsync(invoicePayment);
        }

        public async Task<InvoicePayment> GetInvoicePaymentByIdAsync(int id)
        {
            return await _invoicePaymentRepository.GetByIdAsync(id);
        }

        public async Task<IList<InvoicePayment>> GetCustomerInvoicePaymentListAsync(int customerId)
        {
            var query = from dh in _invoicePaymentRepository.Table
                        where dh.WalletCustomerId == customerId
                        select dh;

            return await query.ToListAsync();
        }

        public async Task<IPagedList<InvoicePayment>> GetAllInvoicePaymentAsync(int customerId = 0, string invoiceReference = null,
            DateTime? paidFromUtc = null, DateTime? paidToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from dh in _invoicePaymentRepository.Table
                        where (customerId == 0 || dh.WalletCustomerId == customerId) &&
                            (string.IsNullOrWhiteSpace(invoiceReference) || dh.InvoiceReference.Contains(invoiceReference)) &&
                            (paidFromUtc == null || dh.PaymentDateUtc.Date >= paidFromUtc.Value.Date) &&
                            (paidToUtc == null || dh.PaymentDateUtc.Date <= paidToUtc.Value.Date)
                        orderby dh.PaymentDateUtc descending
                        select dh;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
