using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using Nop.Services.Stores;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public class SpecialDeliveryCapacityService : ISpecialDeliveryCapacityService
    {
        #region Fields

        private readonly IStoreMappingService _storeMappingService;
        private readonly IRepository<DeliverySlot> _deliverySlotRepository;
        private readonly IRepository<SpecialDeliveryCapacity> _specialDeliveryCapacityRepository;

        #endregion

        #region Ctor

        public SpecialDeliveryCapacityService(
            IStoreMappingService storeMappingService,
            IRepository<DeliverySlot> deliverySlotRepository,
            IRepository<SpecialDeliveryCapacity> specialDeliveryCapacityRepository)
        {
            _storeMappingService = storeMappingService;
            _deliverySlotRepository = deliverySlotRepository;
            _specialDeliveryCapacityRepository = specialDeliveryCapacityRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteSpecialDeliveryCapacityAsync(SpecialDeliveryCapacity specialDeliveryCapacity)
        {
            await _specialDeliveryCapacityRepository.DeleteAsync(specialDeliveryCapacity);
        }

        public async Task InsertSpecialDeliveryCapacityAsync(SpecialDeliveryCapacity specialDeliveryCapacity)
        {
            await _specialDeliveryCapacityRepository.InsertAsync(specialDeliveryCapacity);
        }

        public async Task UpdateSpecialDeliveryCapacityAsync(SpecialDeliveryCapacity specialDeliveryCapacity)
        {
            await _specialDeliveryCapacityRepository.UpdateAsync(specialDeliveryCapacity);
        }

        public async Task<SpecialDeliveryCapacity> GetSpecialDeliveryCapacityByIdAsync(int specialDeliveryCapacityId)
        {
            if (specialDeliveryCapacityId == 0)
                return null;

            return await _specialDeliveryCapacityRepository.GetByIdAsync(specialDeliveryCapacityId, cache => default);
        }

        public async Task<SpecialDeliveryCapacity> GetSpecialDeliveryCapacityAsync(DateTime specialDate, int deliverySlotId, int shippingMethodId)
        {
            var query = from sdc in _specialDeliveryCapacityRepository.Table
                        join ds in _deliverySlotRepository.Table on sdc.DeliverySlotId equals ds.Id
                        where !ds.Deleted &&
                        sdc.SpecialDate.Date == specialDate.Date &&
                        sdc.DeliverySlotId == deliverySlotId &&
                        sdc.ShippingMethodId == shippingMethodId
                        select sdc;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<SpecialDeliveryCapacity>> SearchSpecialDeliveryCapacitiesAsync(DateTime? specialFromDate = null,
            DateTime? specialToDate = null, int deliverySlotId = 0, int shippingMethodId = 0, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (specialFromDate.HasValue)
                specialFromDate = specialFromDate.Value.Date;
            if (specialToDate.HasValue)
                specialToDate = specialToDate.Value.Date;

            var query = from sdc in _specialDeliveryCapacityRepository.Table
                        join ds in _deliverySlotRepository.Table on sdc.DeliverySlotId equals ds.Id
                        where !ds.Deleted &&
                        (specialFromDate == null || sdc.SpecialDate.Date >= specialFromDate) &&
                        (specialToDate == null || sdc.SpecialDate.Date <= specialToDate) &&
                        (deliverySlotId == 0 || sdc.DeliverySlotId == deliverySlotId) &&
                        (shippingMethodId == 0 || sdc.ShippingMethodId == shippingMethodId)
                        orderby sdc.SpecialDate descending, ds.DisplayOrder
                        select sdc;

            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
