using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Payments.bKash.Models;

namespace NopStation.Plugin.Payments.bKash.Services
{
    public interface IBkashService
    {
        Task<PaymentResponse> CreatePaymentAsync(Order order);

        Task<PaymentResponse> ExecutePaymentAsync(Order order);
    }
}