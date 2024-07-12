using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using NopStation.Plugin.Widgets.DeliveryScheduler.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Models.Order;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Factories
{
    public class DeliverySchedulerModelFactory : IDeliverySchedulerModelFactory
    {
        private readonly IDeliverySlotService _deliverySlotService;
        private readonly ISpecialDeliveryCapacityService _specialDeliveryCapacityService;
        private readonly IDeliveryCapacityService _deliveryCapacityService;
        private readonly DeliverySchedulerSettings _deliverySchedulerSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICategoryService _categoryService;
        private readonly ISpecialDeliveryOffsetService _specialDeliveryOffsetService;
        private readonly IOrderDeliverySlotService _orderDeliverySlotService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ISettingService _settingService;

        public DeliverySchedulerModelFactory(IDeliverySlotService deliverySlotService,
            ISpecialDeliveryCapacityService specialDeliveryCapacityService,
            IDeliveryCapacityService deliveryCapacityService,
            DeliverySchedulerSettings deliverySchedulerSettings,
            ILocalizationService localizationService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICategoryService categoryService,
            ISpecialDeliveryOffsetService specialDeliveryOffsetService,
            IOrderDeliverySlotService orderDeliverySlotService,
            IGenericAttributeService genericAttributeService,
            ISettingService settingService)
        {
            _deliverySlotService = deliverySlotService;
            _specialDeliveryCapacityService = specialDeliveryCapacityService;
            _deliveryCapacityService = deliveryCapacityService;
            _deliverySchedulerSettings = deliverySchedulerSettings;
            _localizationService = localizationService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;
            _categoryService = categoryService;
            _specialDeliveryOffsetService = specialDeliveryOffsetService;
            _orderDeliverySlotService = orderDeliverySlotService;
            _genericAttributeService = genericAttributeService;
            _settingService = settingService;
        }

        public async Task<DeliverySlotDetailsModel> PrepareDeliverySlotDetailsModel(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException(nameof(shippingMethod));

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var categoryIds = await GetShoppingCartItemCategories(cart);
            var categoryOffset = await _specialDeliveryOffsetService.GetMaximumCategoryOffset(categoryIds);

            var maxOffset = Math.Max(categoryOffset, _deliverySchedulerSettings.DisplayDayOffset);
            var startDate = DateTime.Now.Date.AddDays(maxOffset);
            var endDate = startDate.AddDays(_deliverySchedulerSettings.NumberOfDaysToDisplay);

            var capacities = await _deliveryCapacityService.SearchDeliveryCapacitiesAsync(shippingMethodId: shippingMethod.Id);
            var slots = await _deliverySlotService.SearchDeliverySlotsAsync(shippingMethodId: shippingMethod.Id, active: true, storeId: _storeContext.GetCurrentStore().Id);
            var specialCapacities = await _specialDeliveryCapacityService.SearchSpecialDeliveryCapacitiesAsync(startDate, endDate, shippingMethodId: shippingMethod.Id, storeId: _storeContext.GetCurrentStore().Id);
            var bookedCapacities = await _orderDeliverySlotService.SearchOrderDeliverySlots(startDate, endDate, shippingMethod.Id);

            var model = new DeliverySlotDetailsModel();
            model.ShowRemainingCapacity = _deliverySchedulerSettings.ShowRemainingCapacity;
            model.ShippingMethodId = shippingMethod.Id;
            var savedSlotInfo = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                DeliverySchedulerDefaults.DeliverySlotInfo, _storeContext.GetCurrentStore().Id);

            if (!string.IsNullOrWhiteSpace(savedSlotInfo))
            {
                var tokens = savedSlotInfo.Split("___", StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 3)
                {
                    if (DateTime.TryParse(tokens[0], out var date))
                        model.SavedDeliveryDate = date.Date;
                    if (int.TryParse(tokens[1], out var slid))
                        model.SavedSlotId = slid;
                    if (int.TryParse(tokens[2], out var smid))
                        model.SavedShippingMethodId = smid;
                }
            }

            for (var i = 0; i < _deliverySchedulerSettings.NumberOfDaysToDisplay; i++)
            {
                var cm = new List<SlotCellModel>();
                var slotDate = startDate.AddDays(i);

                foreach (var slot in slots)
                {
                    var originalCapacity = GetCapacity(capacities, slot, slotDate, specialCapacities);
                    var bookedCapacity = bookedCapacities
                        .Where(x => x.DeliverySlotId == slot.Id && x.DeliveryDate.Date == slotDate.Date)
                        .Count();

                    cm.Add(new SlotCellModel()
                    {
                        SlotId = slot.Id,
                        Capacity = Math.Max(originalCapacity - bookedCapacity, 0),
                        SlotDate = slotDate,
                        SlotName = await _localizationService.GetLocalizedAsync(slot, x => x.TimeSlot)
                    });
                }

                model.DeliveryCapacities.Add(new KeyValuePair<string, List<SlotCellModel>>(slotDate.ToString(_deliverySchedulerSettings.DateFormat), cm));
            }

            return model;
        }

        private async Task<List<int>> GetShoppingCartItemCategories(IList<ShoppingCartItem> cart)
        {
            var ids = new List<int>();
            foreach (var sci in cart)
            {
                var categories = await _categoryService.GetProductCategoriesByProductIdAsync(sci.ProductId);
                if (categories.Any())
                    ids.AddRange(categories.Select(x => x.CategoryId).ToList());
            }

            return ids.Distinct().ToList();
        }

        private int GetCapacity(IPagedList<DeliveryCapacity> capacities, DeliverySlot slot,
            DateTime slotDate, IEnumerable<SpecialDeliveryCapacity> specialCapacities)
        {
            var specialCapacity = specialCapacities.FirstOrDefault(x => x.DeliverySlotId == slot.Id &&
                x.SpecialDate.Date == slotDate.Date);

            if (specialCapacity != null)
                return specialCapacity.Capacity;

            var deliveryCapacity = capacities.FirstOrDefault(x => x.DeliverySlotId == slot.Id);
            if (deliveryCapacity == null)
                return 0;

            return slotDate.DayOfWeek switch
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

        public async Task<OrderDeliveryDetailsModel> PreparedOrderDeliverySlotModelAsync(OrderDeliverySlot orderDeliverySlot)
        {
            var model = new OrderDeliveryDetailsModel();

            var deliverySlot = await _deliverySlotService.GetDeliverySlotByIdAsync(orderDeliverySlot.DeliverySlotId);
            model.DeliverySlot = await _localizationService.GetLocalizedAsync(deliverySlot, x => x.TimeSlot);
            model.ShippingDate = orderDeliverySlot.DeliveryDate.ToString(_deliverySchedulerSettings.DateFormat);

            return model;
        }
    }
}
