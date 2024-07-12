using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using Nop.Services.Stores;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public class DeliverySlotService : IDeliverySlotService
    {
        #region Fields

        private readonly IRepository<DeliverySlot> _deliverySlotRepository;
        private readonly IRepository<DeliverySlotShippingMethodMapping> _dssmmRepository;
        private readonly IRepository<ShippingMethod> _shippingMethodRepository;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public DeliverySlotService(IRepository<DeliverySlot> deliverySlotRepository,
            IRepository<DeliverySlotShippingMethodMapping> dssmmRepository,
            IRepository<ShippingMethod> shippingMethodRepository,
            IStoreMappingService storeMappingService)
        {
            _deliverySlotRepository = deliverySlotRepository;
            _dssmmRepository = dssmmRepository;
            _shippingMethodRepository = shippingMethodRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        public async Task DeleteDeliverySlotAsync(DeliverySlot deliverySlot)
        {
            await _deliverySlotRepository.DeleteAsync(deliverySlot);
        }

        public async Task InsertDeliverySlotAsync(DeliverySlot deliverySlot)
        {
            await _deliverySlotRepository.InsertAsync(deliverySlot);
        }

        public async Task UpdateDeliverySlotAsync(DeliverySlot deliverySlot)
        {
            await _deliverySlotRepository.UpdateAsync(deliverySlot);
        }

        public async Task<DeliverySlot> GetDeliverySlotByIdAsync(int deliverySlotId)
        {
            if (deliverySlotId == 0)
                return null;

            return await _deliverySlotRepository.GetByIdAsync(deliverySlotId, cache => default);
        }

        public bool IsActiveDeliverySlotExits()
        {
            var query = _deliverySlotRepository.Table;
            if (query.Any(q => q.Deleted == false && q.Active == true))
            {
                return true;
            }
            return false;
        }

        public async Task<IPagedList<DeliverySlot>> SearchDeliverySlotsAsync(int shippingMethodId = 0, bool? active = null,
            int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from ds in _deliverySlotRepository.Table
                        where !ds.Deleted &&
                        (active == null || ds.Active == active.Value)
                        select ds;

            if (shippingMethodId > 0)
            {
                query = from ds in query
                        where !ds.LimitedToShippingMethod || _dssmmRepository.Table.Any(sm =>
                          sm.DeliverySlotId == ds.Id && sm.ShippingMethodId == shippingMethodId)
                        select ds;
            }

            query = await _storeMappingService.ApplyStoreMapping(query, storeId);
            query = query.OrderBy(e => e.DisplayOrder);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual IList<int> GetShippingMethodIdsWithAccess(DeliverySlot deliverySlot)
        {
            if (deliverySlot == null)
                throw new ArgumentNullException(nameof(deliverySlot));

            var query = from q in _dssmmRepository.Table
                        where q.DeliverySlotId == deliverySlot.Id
                        select q.ShippingMethodId;

            return query.ToList();
        }

        public virtual async Task<IList<DeliverySlotShippingMethodMapping>> GetDeliverySlotShippingMethodMappingsAsync(DeliverySlot deliverySlot)
        {
            if (deliverySlot == null)
                throw new ArgumentNullException(nameof(deliverySlot));

            var query = from q in _dssmmRepository.Table
                        where q.DeliverySlotId == deliverySlot.Id
                        select q;

            return await query.ToListAsync();
        }

        public virtual async Task DeleteDeliverySlotShippingMethodMappingAsync(DeliverySlotShippingMethodMapping mapping)
        {
            await _dssmmRepository.DeleteAsync(mapping);
        }

        public virtual async Task InsertDeliverySlotShippingMethodMappingAsync(DeliverySlotShippingMethodMapping mapping)
        {
            await _dssmmRepository.InsertAsync(mapping);
        }

        public virtual async Task InsertDeliverySlotShippingMethodMappingAsync(DeliverySlot deliverySlot, int shippingMethodId)
        {
            var mapping = new DeliverySlotShippingMethodMapping()
            {
                DeliverySlotId = deliverySlot.Id,
                ShippingMethodId = shippingMethodId
            };

            await InsertDeliverySlotShippingMethodMappingAsync(mapping);
        }

        #endregion
    }
}
