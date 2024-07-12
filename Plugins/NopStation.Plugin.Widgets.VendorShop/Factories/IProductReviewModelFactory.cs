using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorShop.Models.ProfileTabs;

namespace NopStation.Plugin.Widgets.VendorShop.Factories
{
    public interface IProductReviewModelFactory
    {
        Task<VendorProfileModel> PrepareReviewsModelAsync(int vendorId, VendorReviewsCommand command);
    }
}
