using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public interface IVendorProfileService
    {
        public Task<VendorProfile> GetVendorProfileAsync(int vendorId, int storeId = 0);
        public Task SaveVendorProfileAsync(VendorProfile vendorProfile);
        Task<IPagedList<ProductReview>> GetVendorProductReviewsAsync(int vendorId, int filterBy = 0, int orderBy = 0, int page = 1, int pageSize = int.MaxValue);
    }
}
