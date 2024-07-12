using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;
using Nop.Services.Payments;
using NopStation.Plugin.Payments.Quickstream.Models;

namespace NopStation.Plugin.Payments.Quickstream.Services
{
    public interface IQuickStreamPaymentService
    {
        Task<string> GetSingleUseTokenAsync(IFormCollection form);

        Task<TakePaymentResponseBody> CompletePaymentAsync(ProcessPaymentRequest processPaymentRequest);

        Task<IList<Card>> GetAcceptCardsWithSurChargeAsync(int storeId);

        Task<TakePaymentResponseBody> GetTransactionByReceiptNumberAsync(RefundPaymentRequest refundPaymentRequest);

        Task<TakePaymentResponseBody> RefundAsync(RefundPaymentRequest refundPaymentRequest);

        Task UpdateOrderPaymentStatusAsync();
    }
}
