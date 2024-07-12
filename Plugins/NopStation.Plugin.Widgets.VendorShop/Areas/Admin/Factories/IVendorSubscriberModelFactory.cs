using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.Subscriber;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories
{
    public interface IVendorSubscriberModelFactory
    {
        Task<VendorSubscriberSearchModel> PrepareVendorSubscriberSearchModelAsync(VendorSubscriberSearchModel searchModel);
        Task<VendorSubscriberListModel> PrepareVendorSubscriberListModelAsync(VendorSubscriberSearchModel searchModel);
    }
}
