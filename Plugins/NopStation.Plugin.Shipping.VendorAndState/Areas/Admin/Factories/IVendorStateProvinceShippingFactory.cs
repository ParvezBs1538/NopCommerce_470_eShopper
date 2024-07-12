using System.Threading.Tasks;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Factories
{
    public interface IVendorStateProvinceShippingFactory
    {
        Task<VendorStateProvinceShippingListModel> PrepareVendorStateProvinceShippingListModelAsync(VendorStateProvinceShippingSearchModel searchModel);
    }
}