using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Payments.POLiPay.Models;

namespace NopStation.Plugin.Payments.POLiPay.Factories
{
    public interface IPolypayModelFactory
    {
        Task<PaymentUrlRequest> PreparePaymentUrlRequest(Order order);
    }
}
