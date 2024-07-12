using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public class OrderDeliverySlotModelFactory : IOrderDeliverySlotModelFactory
    {
        private readonly IOrderDeliverySlotService _orderDeliverySlotService;
        private readonly IDeliverySlotService _deliverySlotService;
        private readonly IShippingService _shippingService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;

        public OrderDeliverySlotModelFactory(IOrderDeliverySlotService orderDeliverySlotService,
            IDeliverySlotService deliverySlotService,
            IShippingService shippingService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService)
        {
            _orderDeliverySlotService = orderDeliverySlotService;
            _deliverySlotService = deliverySlotService;
            _shippingService = shippingService;
            _settingService = settingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
        }

        public async Task<OrderDeliverySlotListModel> PrepareOrderDeliverySlotModelListAsync(OrderDeliverySlotSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var orderList = await _orderDeliverySlotService.SearchOrderDeliverySlots(
                searchModel.SearchStartDate,
                searchModel.SearchEndTime, 
                searchModel.SearchShippingMethodId,
                searchModel.SearchDeliverySlotId, 
                searchModel.Page - 1, searchModel.PageSize);

            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
            var slots = await _deliverySlotService.SearchDeliverySlotsAsync();

            var model = new OrderDeliverySlotListModel().PrepareToGrid(searchModel, orderList, () =>
            {
                return orderList.Select(item => new OrderDeliverySlotModel()
                {
                    DeliverySlot = slots.FirstOrDefault(x => x.Id == item.DeliverySlotId)?.TimeSlot,
                    ShippingMethod = shippingMethods.FirstOrDefault(x => x.Id == item.ShippingMethodId)?.Name,
                    DeliveryDate = item.DeliveryDate,
                    Id = item.OrderId,
                    DeliverySlotId = item.DeliverySlotId
                });
            });

            return model;
        }

        public async Task<OrderDeliverySlotSearchModel> PrepareOrderDeliverySlotSearchModelAsync(OrderDeliverySlotSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
            var slots = await _deliverySlotService.SearchDeliverySlotsAsync();

            searchModel.AvailableDeliverySlots = slots.Select(x => new SelectListItem
            {
                Text = x.TimeSlot,
                Value = x.Id.ToString()
            }).ToList();

            searchModel.AvailableDeliverySlots.Insert(0, new SelectListItem()
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.DeliverySlot.All")
            });

            searchModel.AvailableShippingMethods = shippingMethods.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            searchModel.AvailableShippingMethods.Insert(0, new SelectListItem()
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.ShippingMethod.All")
            });

            return searchModel;
        }
    }
}
