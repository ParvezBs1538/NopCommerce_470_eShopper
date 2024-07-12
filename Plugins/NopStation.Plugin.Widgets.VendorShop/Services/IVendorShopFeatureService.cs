using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public interface IVendorShopFeatureService
    {
        Task InsertAsync(VendorFeatureMapping vendorFeatureMapping);
        Task UpdateAsync(VendorFeatureMapping vendorFeatureMapping);
        Task DeleteAsync(VendorFeatureMapping vendorFeatureMapping);
        Task<bool> IsEnableVendorShopAsync(int vendorId);
        Task<VendorFeatureMapping> GetVendorFeatureMappingByIdAsync(int id);
        Task<VendorFeatureMapping> GetVendorFeatureMappingByVendorIdAsync(int vendorId);
    }
}