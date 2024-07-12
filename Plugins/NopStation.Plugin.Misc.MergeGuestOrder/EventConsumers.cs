using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Misc.MergeGuestOrder.Services;
using Nop.Services.Events;
using Nop.Services.Localization;

namespace NopStation.Plugin.Misc.MergeGuestOrder
{
    public class EventConsumers : IConsumer<CustomerRegisteredEvent>
    {
        private readonly MergeGuestOrderSettings _mergeGuestOrderSettings;
        private readonly IOrderServiceCustom _orderServiceCustom;
        private readonly ILocalizationService _localizationService;

        public EventConsumers(MergeGuestOrderSettings mergeGuestOrderSettings,
            IOrderServiceCustom orderServiceCustom,
            ILocalizationService localizationService)
        {
            _mergeGuestOrderSettings = mergeGuestOrderSettings;
            _orderServiceCustom = orderServiceCustom;
            _localizationService = localizationService;
        }

        public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            if (!_mergeGuestOrderSettings.EnablePlugin)
                return;

            var checkIn = (CheckEmailInAddress)_mergeGuestOrderSettings.CheckEmailInAddressId;

            var i = 0;
            while (true)
            {
                var orders = await _orderServiceCustom.SearchOrders(eventMessage.Customer.Email, checkIn, i, 200);
                if (!orders.Any())
                    break;

                var orderNotes = new List<OrderNote>();
                for (var j = 0; j < orders.Count; j++)
                {
                    var order = orders[j];
                    order.CustomerId = eventMessage.Customer.Id;

                    if (_mergeGuestOrderSettings.AddNoteToOrderOnMerge)
                    {
                        orderNotes.Add(new OrderNote
                        {
                            CreatedOnUtc = System.DateTime.UtcNow,
                            OrderId = order.Id,
                            Note = string.Format(await _localizationService.GetResourceAsync("NopStation.MergeGuestOrder.OrderNote"), eventMessage.Customer.Email)
                        });
                    }
                }
                i++;

                await _orderServiceCustom.UpdateOrdersAsync(orders);
                if (orderNotes.Any())
                    await _orderServiceCustom.InsertOrderNotesAsync(orderNotes);
            }
        }
    }
}
