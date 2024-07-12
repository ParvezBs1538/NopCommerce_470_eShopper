using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Factories
{
    public class PickupInStoreDeliveryManageModelFactory : IPickupInStoreDeliveryManageModelFactory
    {
        private readonly IPickupInStoreDeliveryManageService _pickupInStoreDeliveryManageService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public PickupInStoreDeliveryManageModelFactory(IPickupInStoreDeliveryManageService pickupInStoreDeliveryManageService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IDateTimeHelper dateTimeHelper)
        {
            _pickupInStoreDeliveryManageService = pickupInStoreDeliveryManageService;
            _localizationService = localizationService;
            _orderService = orderService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<PickupInStoreDeliveryManageListModel> PreparePickupInStoreDeliveryManageListModelAsync(PickupInStoreDeliveryManageSearchModel searchModel)
        {
            var ordersArray = (await _pickupInStoreDeliveryManageService.GetAllOrdersAsync(searchModel)).ToArray();
            var pageSize = searchModel.PageSize;
            var pageIndex = searchModel.Page;
            if (pageSize <= 0)
            {
                pageSize = 1;
            }
            var totalPage = ordersArray.Length % pageSize == 0 ? ordersArray.Length / pageSize : (ordersArray.Length / pageSize) + 1;
            if (pageIndex > totalPage)
            {
                pageIndex = totalPage;
            }
            var totalCount = ordersArray.Length;
            var startIndex = 0;
            var endIndex = 10;
            if (pageIndex > 0)
            {
                startIndex = (pageIndex - 1) * pageSize;
                endIndex = startIndex + pageSize;
            }
            if (endIndex >= totalCount)
            {
                endIndex = totalCount < 0 ? 0 : totalCount;
            }
            var orders = ordersArray[startIndex..endIndex].Select(x =>
            {
                var order = _orderService.GetOrderByIdAsync(x.OrderId).Result;
                var model = new PickupInStoreDeliveryManageModel
                {
                    Id = x.Id,
                    OrderId = x.OrderId,
                    OrderNumber = order.CustomOrderNumber,
                    PickUpStatus = x.PickUpStatusTypeId.ToString(),
                    ReadyForPickupMarkedAtUtc = x.ReadyForPickupMarkedAtUtc.HasValue ? _dateTimeHelper.ConvertToUserTimeAsync(x.ReadyForPickupMarkedAtUtc.Value, DateTimeKind.Utc).Result.ToString() : string.Empty,
                    PickupUpAtUtc = x.CustomerPickedUpAtUtc.HasValue ? _dateTimeHelper.ConvertToUserTimeAsync(x.CustomerPickedUpAtUtc.Value, DateTimeKind.Utc).Result.ToString() : string.Empty,
                    PickUpStatusTypeId = x.PickUpStatusTypeId,
                    OrderDate = order != null ? _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc).Result.ToString() : string.Empty,
                    NopOrderStatus = order != null ? order.OrderStatus.ToString() : string.Empty,
                };
                return model;
            }).ToList();

            var pagedList = new PagedList<PickupInStoreDeliveryManageModel>(orders, searchModel.Page - 1, pageSize, totalCount);

            var model = new PickupInStoreDeliveryManageListModel().PrepareToGrid(searchModel, pagedList, () =>
            {
                return orders.Select(x =>
                {
                    return new PickupInStoreDeliveryManageModel
                    {
                        Id = x.Id,
                        OrderId = x.OrderId,
                        OrderNumber = x.OrderNumber,
                        PickUpStatusTypeId = x.PickUpStatusTypeId,
                        NopOrderStatus = x.NopOrderStatus,
                        PickupUpAtUtc = x.PickupUpAtUtc,
                        OrderDate = x.OrderDate,
                        ReadyForPickupMarkedAtUtc = x.ReadyForPickupMarkedAtUtc,
                        PickUpStatus = GetStatusAsync(x.PickUpStatus).Result
                    };
                });
            });
            return model;
        }

        protected async Task<string> GetStatusAsync(string sId)
        {
            if (sId == "1")
                return await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.1");
            else if (sId == "2")
                return await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.2");
            else if (sId == "3")
                return await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.3");
            else if (sId == "4")
                return await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.4");
            else
                return string.Empty;
        }
        protected async Task<string> GetNopOrderStatusAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return order.OrderStatus.ToString();
            }
            else
                return string.Empty;
        }

        protected async Task<IList<SelectListItem>> PrepareAvailableStatusAsync()
        {
            var availableStatus = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text =await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.SearchModel.All"),
                    Value = "0"
                },
                new SelectListItem
                {
                    Text =await  _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.1"),
                    Value = "1"
                },
                new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.2"),
                    Value = "2"
                },
                new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.3"),
                    Value = "3"
                },
                new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.4"),
                    Value = "4"
                }
            };
            return availableStatus;
        }
        public PickupInStoreDeliveryManageSearchModel PreparePickupInStoreDeliveryManageSearchModel(PickupInStoreDeliveryManageSearchModel searchModel)
        {
            searchModel.AvailableSearchStatus = GetAvailableSearchStatusAsync().Result;
            searchModel.AvailableSearchStatus.Insert(0, new SelectListItem
            {
                Text = _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.SearchModel.All").Result,
                Value = "0"
            });
            return searchModel;
        }

        protected async Task<IList<SelectListItem>> GetAvailableSearchStatusAsync()
        {
            return await PrepareAvailableStatusAsync();
        }
    }
}
