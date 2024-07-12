using System.Threading.Tasks;

namespace NopStation.Plugin.Payments.Afterpay.Services
{
    public interface IAfterpayUpdateService
    {
        Task UpdateOrderPaymentStatusAsync();
    }
}