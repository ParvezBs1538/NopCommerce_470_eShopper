using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Payments;
using NopStation.Plugin.Payments.CopyAndPay.Models;

namespace NopStation.Plugin.Payments.CopyAndPay.Services
{
    public interface ICopyAndPayServices
    {
        Task<Dictionary<string, dynamic>> RequestFormAsync(int orderId, string selectedBrand);

        bool RequestPaymentStatus(int orderId, string id, out ValidatePayment responseData);

        bool RefundPayment(RefundPaymentRequest refundPaymentRequest, out RefundPayment responseData);
    }
}
