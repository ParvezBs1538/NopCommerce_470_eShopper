using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorCommission.Domain;

namespace NopStation.Plugin.Payout.Stripe.Services
{
    public interface IVendorStripePayoutService
    {
        Task<ProcessPayoutResult> ProcessVendorPayoutAsync(ProcessPayoutRequest request);
    }
}
