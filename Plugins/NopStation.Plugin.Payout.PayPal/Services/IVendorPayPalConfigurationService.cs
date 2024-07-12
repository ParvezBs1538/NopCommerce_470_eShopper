using System.Threading.Tasks;
using NopStation.Plugin.Payout.PayPal.Domain;

namespace NopStation.Plugin.Payout.PayPal.Services
{
    public interface IVendorPayPalConfigurationService
    {
        Task<VendorPayPalConfiguration> GetVendorPayPalConfigurationByVendorIdAsync(int vendorId);
        Task InsertVendorPaypalConfigutationAsync(VendorPayPalConfiguration vendorPayPalConfiguration);
        Task UpdateVendorPaypalConfigutationAsync(VendorPayPalConfiguration vendorPayPalConfiguration);
    }
}
