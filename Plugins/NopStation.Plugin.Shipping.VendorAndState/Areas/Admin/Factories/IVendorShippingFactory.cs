using System.Threading.Tasks;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models;
using NopStation.Plugin.Shipping.VendorAndState.Domain;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Factories
{
    public interface IVendorShippingFactory
    {
        Task<VendorShippingSearchModel> PrepareVendorShippingSearchModelAsync(VendorShippingSearchModel searchModel);

        Task<VendorShippingListModel> PrepareVendorShippingListModelAsync(VendorShippingSearchModel searchModel);

        Task<VendorShippingModel> PrepareVendorShippingModelAsync(VendorShippingModel model, VendorShipping vendorShipping, bool excludeProperties = false);
    }
}