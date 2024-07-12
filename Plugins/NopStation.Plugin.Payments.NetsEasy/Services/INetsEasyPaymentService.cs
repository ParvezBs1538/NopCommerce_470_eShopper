using System.Threading.Tasks;
using Nop.Services.Payments;
using NopStation.Plugin.Payments.NetsEasy.Models;
using NopStation.Plugin.Payments.NetsEasy.Models.Response;

namespace NopStation.Plugin.Payments.NetsEasy.Services
{
    public interface INetsEasyPaymentService
    {
        Task<PublicInfoModel> CreatePaymentAsync(ProcessPaymentRequest paymentRequest, int storeId);

        Task<Payment> RetrievePaymentAsync(string paymentId, int storeId);

        Task<ChargePaymentResponseModel> ChargePaymentAsync(CapturePaymentRequest capturePaymentRequest, int storeId);

        Task<RefundPaymentResponseModel> RefundChargeAsync(RefundPaymentRequest refundPaymentRequest, int storeId);

        Task<bool> CancelPaymentAsync(VoidPaymentRequest voidPaymentRequest, int storeId);
        Task<ChargeSubscriptionsResponseModel> ChargeSubscriptionAsync(ChargeSubscriptions chargeSubscriptions, int storeId);
        Task<RetrieveSubscriptionChargeModel> RetrievesubscriptionChargeAsync(string bulkId, int storeId);
        Task<NetsPaymentModel> VerifyPaymentAsync(string paymentId, int storeId);
    }
}