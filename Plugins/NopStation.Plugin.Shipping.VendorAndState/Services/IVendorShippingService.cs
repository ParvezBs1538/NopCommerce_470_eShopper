using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Shipping.VendorAndState.Domain;

namespace NopStation.Plugin.Shipping.VendorAndState.Services
{
    public interface IVendorShippingService
    {
        Task<VendorShipping> GetVendorShippingByIdAsync(int id);

        Task<VendorShipping> GetVendorShippingByVendorIdAndShippingMethodIdAsync(int vendorId, int shippingMethodId);

        Task InsertVendorShippingAsync(VendorShipping vendorShipping);

        Task UpdateVendorShippingAsync(VendorShipping vendorShipping);

        Task DeleteVendorShippingAsync(VendorShipping vendorShipping);

        Task<IPagedList<VendorShipping>> GetAllVendorShippingsAsync(int shippingMethodId = 0, int vendorId = 0,
            bool? hideShippingMethod = null, bool? sellerSideDelivery = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}