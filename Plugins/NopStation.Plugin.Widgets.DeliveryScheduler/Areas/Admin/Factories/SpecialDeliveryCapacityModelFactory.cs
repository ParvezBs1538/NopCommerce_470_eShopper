using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public class SpecialDeliveryCapacityModelFactory : ISpecialDeliveryCapacityModelFactory
    {
        private readonly IStoreService _storeService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IDeliverySlotService _deliverySlotService;
        private readonly ISpecialDeliveryCapacityService _specialDeliveryCapacityService;
        private readonly IShippingService _shippingService;
        private readonly DeliverySchedulerSettings _deliverySchedulerSettings;

        public SpecialDeliveryCapacityModelFactory(
            IStoreService storeService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IDeliverySlotService deliverySlotService,
            ISpecialDeliveryCapacityService specialDeliveryCapacityService,
            IShippingService shippingService,
            DeliverySchedulerSettings deliverySchedulerSettings)
        {
            _storeService = storeService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _deliverySlotService = deliverySlotService;
            _specialDeliveryCapacityService = specialDeliveryCapacityService;
            _shippingService = shippingService;
            _deliverySchedulerSettings = deliverySchedulerSettings;
        }

        public async Task<SpecialDeliveryCapacityListModel> PrepareCapacityListModelAsync(SpecialDeliveryCapacitySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var capacities = await _specialDeliveryCapacityService.SearchSpecialDeliveryCapacitiesAsync(
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new SpecialDeliveryCapacityListModel().PrepareToGridAsync(searchModel, capacities, () =>
            {
                return capacities.SelectAwait(async capacity =>
                {
                    return await PrepareCapacityModelAsync(null, capacity, true);
                });
            });

            return model;
        }

        public async Task<SpecialDeliveryCapacityModel> PrepareCapacityModelAsync(SpecialDeliveryCapacityModel model, SpecialDeliveryCapacity specialDeliveryCapacity,
            bool excludeProperties = false)
        {
            if (specialDeliveryCapacity != null && model == null)
            {
                model = specialDeliveryCapacity.ToModel<SpecialDeliveryCapacityModel>();
                model.SpecialDateStr = specialDeliveryCapacity.SpecialDate.ToString(_deliverySchedulerSettings.DateFormat);

                var slot = await _deliverySlotService.GetDeliverySlotByIdAsync(specialDeliveryCapacity.DeliverySlotId);
                model.DeliverySlot = slot.TimeSlot;
            }
            else
            {
                model.SpecialDate = DateTime.UtcNow;
            }

            if (!excludeProperties)
            {
                var slots = await _deliverySlotService.SearchDeliverySlotsAsync();
                model.AvailableDeliverySlots = slots.Select(x => new SelectListItem()
                {
                    Text = x.TimeSlot,
                    Value = x.Id.ToString()
                }).ToList();

                var methods = await _shippingService.GetAllShippingMethodsAsync();
                model.AvailableShippingMethods = methods.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
            }

            //prepare available stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, specialDeliveryCapacity, false);

            model.MappedStoreNames = string.Join("; ", await model.SelectedStoreIds.SelectAwait(async storeId => (await _storeService.GetStoreByIdAsync(storeId)).Name).ToListAsync());

            return model;
        }

        public Task<SpecialDeliveryCapacitySearchModel> PrepareCapacitySearchModelAsync(SpecialDeliveryCapacitySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }
    }
}
