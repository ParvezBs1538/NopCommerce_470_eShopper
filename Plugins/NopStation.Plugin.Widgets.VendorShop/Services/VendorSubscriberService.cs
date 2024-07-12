using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.Subscriber;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public class VendorSubscriberService : IVendorSubscriberService
    {
        private readonly IRepository<VendorSubscriber> _repository;

        public VendorSubscriberService(IRepository<VendorSubscriber> repository)
        {
            _repository = repository;
        }
        public async Task DeleteVendorSubscriberAsync(VendorSubscriber vendorSubscriber)
        {
            await _repository.DeleteAsync(vendorSubscriber);
        }

        public async Task DeleteVendorSubscribersAsync(IList<VendorSubscriber> vendorSubscribers)
        {
            if (vendorSubscribers == null)
                throw new ArgumentNullException(nameof(vendorSubscribers));

            foreach (var vendorSubscriber in vendorSubscribers)
                await DeleteVendorSubscriberAsync(vendorSubscriber);
        }

        public async Task<VendorSubscriber> GetVendorSubscriberByIdAsync(int id)
        {
            if (id == 0)
                return null;
            else
                return await _repository.GetByIdAsync(id);

        }

        public async Task<IList<VendorSubscriber>> GetVendorSubscribersByIdsAsync(IList<int> ids, int vendorId)
        {
            return await _repository.Table.Where(x => ids.Contains(x.Id) && x.VendorId == vendorId).ToListAsync();
        }

        public async Task<IList<VendorSubscriber>> GetVendorSubscribersByVendorIdAsync(int vendorId, int storeId = 0)
        {
            if (vendorId == 0)
                return null;
            else
                return await _repository.Table.WhereAwait(async x => await Task.FromResult(x.Id == vendorId) && storeId == 0 || x.StoreId == storeId).ToListAsync();
        }

        public async Task InsertVendorSubscriberAsync(VendorSubscriber vendorSubscriber)
        {
            await _repository.InsertAsync(vendorSubscriber);
        }

        public async Task<bool> IsSubscribedAsync(int vendorId, int customerId, int storeId = 0)
        {
            return (await _repository.Table.Where(x => x.VendorId == vendorId && x.CustomerId == customerId && storeId == 0 || x.StoreId == storeId).FirstOrDefaultAsync()) != null;
        }
        public async Task<VendorSubscriber> GetVendorSubscriberAsync(int vendorId, int customerId, int storeId = 0)
        {
            return await _repository.Table.Where(x => x.VendorId == vendorId && x.CustomerId == customerId && storeId == 0 || x.StoreId == storeId).FirstOrDefaultAsync();
        }

        public async Task<IPagedList<VendorSubscriber>> GetVendorSubscribersAsync(VendorSubscriberSearchModel searchModel, int vendorId, int storeId = 0)
        {
            var query = _repository.Table;
            query = query.Where(x => x.VendorId == vendorId && storeId == 0 || x.StoreId == storeId);

            var pagedList = await query.ToPagedListAsync(
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            return pagedList;
        }
        public async Task<IPagedList<VendorSubscriber>> GetVendorSubscribersAsync(int vendorId, int storeId = 0, int page = 1, int pageSize = int.MaxValue)
        {
            var query = _repository.Table;
            query = query.Where(x => x.VendorId == vendorId && storeId == 0 || x.StoreId == storeId);

            var pagedList = await query.ToPagedListAsync(
                pageIndex: page - 1,
                pageSize: pageSize);

            return pagedList;
        }
    }
}
