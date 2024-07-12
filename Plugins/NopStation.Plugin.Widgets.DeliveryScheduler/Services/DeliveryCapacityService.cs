using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public class DeliveryCapacityService : IDeliveryCapacityService
    {
        #region Fields

        private readonly IRepository<DeliveryCapacity> _deliveryCapacityRepository;
        private readonly IStoreContext _storeContext;
        private readonly ISpecialDeliveryCapacityService _specialDeliveryCapacityService;
        private readonly IRepository<DeliverySlot> _deliverySlotRepository;
        private readonly IOrderDeliverySlotService _orderDeliverySlotService;

        #endregion

        #region Ctor

        public DeliveryCapacityService(IRepository<DeliveryCapacity> deliveryCapacityRepository,
            IStoreContext storeContext,
            ISpecialDeliveryCapacityService specialDeliveryCapacityService,
            IRepository<DeliverySlot> deliverySlotRepository,
            IOrderDeliverySlotService orderDeliverySlotService)
        {
            _deliveryCapacityRepository = deliveryCapacityRepository;
            _storeContext = storeContext;
            _specialDeliveryCapacityService = specialDeliveryCapacityService;
            _deliverySlotRepository = deliverySlotRepository;
            _orderDeliverySlotService = orderDeliverySlotService;
        }

        #endregion

        #region Methods

        public async Task DeleteDeliveryCapacityAsync(DeliveryCapacity deliveryCapacity)
        {
            await _deliveryCapacityRepository.DeleteAsync(deliveryCapacity);
        }

        public async Task InsertDeliveryCapacityAsync(DeliveryCapacity deliveryCapacity)
        {
            await _deliveryCapacityRepository.InsertAsync(deliveryCapacity);
        }

        public async Task UpdateDeliveryCapacityAsync(DeliveryCapacity deliveryCapacity)
        {
            await _deliveryCapacityRepository.UpdateAsync(deliveryCapacity);
        }

        public async Task<DeliveryCapacity> GetDeliveryCapacityAsync(int deliverySlotId, int shippingMethodId)
        {
            if (deliverySlotId == 0 || shippingMethodId == 0)
                return null;

            return await _deliveryCapacityRepository.Table
                .FirstOrDefaultAsync(x => x.DeliverySlotId == deliverySlotId && x.ShippingMethodId == shippingMethodId);
        }

        public async Task<int> GetDeliveryCapacityForDateSlotAsync(DateTime date, DeliverySlot deliverySlot, int shippingMethodId)
        {
            if (deliverySlot == null || shippingMethodId == 0)
                return 0;

            int capacity;
            if (await _specialDeliveryCapacityService.GetSpecialDeliveryCapacityAsync(date, deliverySlot.Id, shippingMethodId) is SpecialDeliveryCapacity specialCapacity)
                capacity = specialCapacity.Capacity;
            else
            {
                var deliveryCapacity = await GetDeliveryCapacityAsync(deliverySlot.Id, shippingMethodId);
                if (deliveryCapacity == null)
                    return 0;

                capacity = date.DayOfWeek switch
                {
                    DayOfWeek.Sunday => deliveryCapacity.Day1Capacity,
                    DayOfWeek.Monday => deliveryCapacity.Day2Capacity,
                    DayOfWeek.Tuesday => deliveryCapacity.Day3Capacity,
                    DayOfWeek.Wednesday => deliveryCapacity.Day4Capacity,
                    DayOfWeek.Thursday => deliveryCapacity.Day5Capacity,
                    DayOfWeek.Friday => deliveryCapacity.Day6Capacity,
                    DayOfWeek.Saturday => deliveryCapacity.Day7Capacity,
                    _ => 0,
                };
            }

            var storeId = _storeContext.GetCurrentStore().Id;

            var bookedCount = await _orderDeliverySlotService.GetOrderDeliverySlotsByDateAndSlotId(date, deliverySlot.Id, shippingMethodId, storeId);

            return capacity - bookedCount.Count;
        }

        public async Task<IPagedList<DeliveryCapacity>> SearchDeliveryCapacitiesAsync(int shippingMethodId = 0, int deliverySlotId = 0, 
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from dc in _deliveryCapacityRepository.Table
                        where (shippingMethodId == 0 || dc.ShippingMethodId == shippingMethodId) &&
                        (deliverySlotId == 0 || dc.DeliverySlotId == deliverySlotId)
                        select dc;

            query = query.OrderByDescending(e => e.DeliverySlotId);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
