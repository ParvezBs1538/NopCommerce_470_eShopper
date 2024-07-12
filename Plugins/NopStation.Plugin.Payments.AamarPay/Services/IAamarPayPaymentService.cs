using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Payments.AamarPay.Models;

namespace NopStation.Plugin.Payments.AamarPay.Services;

public interface IAamarPayPaymentService
{
    Task<PaymentInitResponseModel> AamarPayPaymentInitAsync(Order order);
}