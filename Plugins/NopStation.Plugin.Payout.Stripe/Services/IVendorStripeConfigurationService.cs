using System.Threading.Tasks;
using NopStation.Plugin.Payout.Stripe.Domain;

namespace NopStation.Plugin.Payout.Stripe.Services
{
    public interface IVendorStripeConfigurationService
    {
        Task<VendorStripeConfiguration> GetVendorStripeConfigurationByVendorIdAsync(int vendorId);
        Task InsertVendorStripeConfigutationAsync(VendorStripeConfiguration vendorStripeConfiguration);
        Task UpdateVendorStripeConfigutationAsync(VendorStripeConfiguration vendorStripeConfiguration);
    }
}
