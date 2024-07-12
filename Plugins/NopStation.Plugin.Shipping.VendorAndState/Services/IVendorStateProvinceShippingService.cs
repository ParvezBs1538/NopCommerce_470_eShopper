using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Shipping.VendorAndState.Domain;

namespace NopStation.Plugin.Shipping.VendorAndState.Services
{
    public interface IVendorStateProvinceShippingService
    {
        Task<VendorStateProvinceShipping> GetVendorStateProvinceShippingByIdAsync(int id);

        Task<VendorStateProvinceShipping> GetVendorStateProvinceShippingByVendorIdAndShippingMethodIdAsync(int vendorId, int shippingMethodId, int stateProvinceId);

        Task InsertVendorStateProvinceShippingAsync(VendorStateProvinceShipping vendorStateProvinceShipping);

        Task UpdateVendorStateProvinceShippingAsync(VendorStateProvinceShipping vendorStateProvinceShipping);

        Task DeleteVendorStateProvinceShippingAsync(VendorStateProvinceShipping vendorStateProvinceShipping);

        Task<IPagedList<VendorStateProvinceShipping>> GetAllVendorStateProvinceShippingsAsync(int shippingMethodId = 0, int vendorId = 0, int stateProvinceId = 0,
            bool? hideShippingMethod = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}