using System.Threading.Tasks;
using Nop.Services.Payments;
using NopStation.Plugin.Payments.Paykeeper.Services.Responses;

namespace NopStation.Plugin.Payments.Paykeeper.Services
{
    public interface IPaykeeperWebRequest
    {
        TokenResponse GetToken();

        Task<InvoiceInformationResponse> GetInvoiceInformationAsync(PostProcessPaymentRequest postProcessPaymentRequest);

        string Refund(RefundPaymentRequest refundPaymentRequest);
    }
}