using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.GemVisaPayment.Models;

namespace Nop.Plugin.Payments.GemVisaPayment.Services
{
    public interface IGemVisaPaymentService
    {
        PurchaseRequestBody GenerateRequestBody(Order order);
        PurchaseResponseBody GetResponse(PurchaseRequestBody purchaseRequestBody);
        VerifyResponseBody VerifyPayment(VerifyRequestBody verifyRequestBody);
    }
}