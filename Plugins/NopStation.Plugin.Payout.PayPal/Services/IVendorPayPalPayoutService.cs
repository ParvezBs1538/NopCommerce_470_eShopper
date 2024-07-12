using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorCommission.Domain;

namespace NopStation.Plugin.Payout.PayPal.Services
{
    public interface IVendorPayPalPayoutService
    {
        Task<ProcessPayoutResult> ProcessPayPalPayoutAsync(ProcessPayoutRequest request);
    }
}
