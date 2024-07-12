using System.Threading.Tasks;
using Nop.Services.Payments;

namespace NopStation.Plugin.Payments.OABIPayment.Services
{
    public interface IOABIPaymentService
    {
        Task<string> GetLink(PostProcessPaymentRequest postProcessPaymentRequest);
    }
}