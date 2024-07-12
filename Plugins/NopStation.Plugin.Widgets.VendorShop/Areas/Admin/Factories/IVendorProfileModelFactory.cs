using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories
{
    public interface IVendorProfileModelFactory
    {
        Task<VendorProfileModel> PrepareVendorProfileModelAsync(VendorProfileModel model, VendorProfile vendorProfile);
    }
}
