using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.Subscriber;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public interface IVendorSubscriberService
    {
        Task<VendorSubscriber> GetVendorSubscriberByIdAsync(int id);
        Task<IList<VendorSubscriber>> GetVendorSubscribersByIdsAsync(IList<int> ids, int vendorId);
        Task<IList<VendorSubscriber>> GetVendorSubscribersByVendorIdAsync(int vendorId, int storeId = 0);
        Task InsertVendorSubscriberAsync(VendorSubscriber vendorSubscriber);
        Task DeleteVendorSubscriberAsync(VendorSubscriber vendorSubscriber);
        Task DeleteVendorSubscribersAsync(IList<VendorSubscriber> vendorSubscribers);
        Task<bool> IsSubscribedAsync(int vendorId, int customerId, int storeId = 0);
        Task<VendorSubscriber> GetVendorSubscriberAsync(int vendorId, int customerId, int storeId = 0);
        Task<IPagedList<VendorSubscriber>> GetVendorSubscribersAsync(VendorSubscriberSearchModel searchModel, int vendorId, int storeId = 0);
        Task<IPagedList<VendorSubscriber>> GetVendorSubscribersAsync(int vendorId, int storeId = 0, int page = 1, int pageSize = int.MaxValue);
    }
}