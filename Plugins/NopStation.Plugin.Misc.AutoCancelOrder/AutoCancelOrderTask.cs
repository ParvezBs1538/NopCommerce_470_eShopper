using NopStation.Plugin.Misc.AutoCancelOrder.Services;
using System;
using Nop.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;
using Nop.Services.Localization;
using Nop.Services.ScheduleTasks;

namespace NopStation.Plugin.Misc.AutoCancelOrder
{
    public class AutoCancelOrderTask : IScheduleTask
    {
        private readonly IOrderCustomService _orderCustomService;
        private readonly AutoCancelOrderSettings _autoCancelOrderSettings;
        private readonly IStoreContext _storeContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;

        public AutoCancelOrderTask(IOrderCustomService orderCustomService,
            AutoCancelOrderSettings autoCancelOrderSettings,
            IStoreContext storeContext,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService)
        {
            _orderCustomService = orderCustomService;
            _autoCancelOrderSettings = autoCancelOrderSettings;
            _storeContext = storeContext;
            _orderProcessingService = orderProcessingService;
            _localizationService = localizationService;
        }

        public async Task ExecuteAsync()
        {
            var searchParams = new List<SearchParam>();
            foreach (var item in _autoCancelOrderSettings.ApplyOnPaymentMethods)
            {
                searchParams.Add(new SearchParam
                {
                    CreatedOnUtc = GetCreatedOnUtc(item),
                    PaymentMethodSystemName = item.SystemName
                });
            }

            if (!searchParams.Any())
                return;

            var notes = new List<OrderNote>();

            var pageIndex = 0;
            while (true)
            {
                var orders = await _orderCustomService.SearchOrders(
                    searchParams: searchParams,
                    oids: _autoCancelOrderSettings.ApplyOnOrderStatuses.ToArray(),
                    sids: _autoCancelOrderSettings.ApplyOnShippingStatuses.ToArray(),
                    storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                    pageIndex: pageIndex,
                    pageSize: 100);

                if (!orders.Any())
                    break;

                for (var i = 0; i < orders.Count; i++)
                {
                    var order = orders[i];
                    await _orderProcessingService.CancelOrderAsync(order, _autoCancelOrderSettings.SendNotificationToCustomer);
                    notes.Add(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = await _localizationService.GetResourceAsync("NopStation.AutoCancelOrder.OrderNote"),
                        OrderId = order.Id
                    });
                }

                pageIndex++;
            }

            if (notes.Any())
                await _orderCustomService.InsertOrderNotesAsync(notes);
        }

        private DateTime GetCreatedOnUtc(PaymentMethodOffset item)
        {
            var date = DateTime.UtcNow;
            date = (UnitType)item.UnitTypeId switch
            {
                UnitType.Hours => date.AddHours(-item.Offset),
                UnitType.Days => date.AddDays(-item.Offset),
                UnitType.Weeks => date.AddDays(-item.Offset * 7),
                _ => date.AddMonths(-item.Offset),
            };
            return date;
        }
    }
}


