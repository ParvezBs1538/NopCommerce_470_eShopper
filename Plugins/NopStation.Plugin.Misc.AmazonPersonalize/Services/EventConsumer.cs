using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;
using NopStation.Plugin.Misc.AmazonPersonalize.Factories;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public class EventConsumer :
        IConsumer<OrderPlacedEvent>,
        IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        private readonly IEventTrackerModelFactory _eventTrackerModelFactory;
        private readonly IEventTrackerService _eventTrackerService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;

        #region Ctor

        public EventConsumer(IEventTrackerModelFactory eventTrackerModelFactory,
            IEventTrackerService eventTrackerService,
            IOrderService orderService,
            IWorkContext workContext)
        {
            _eventTrackerModelFactory = eventTrackerModelFactory;
            _eventTrackerService = eventTrackerService;
            _orderService = orderService;
            _workContext = workContext;
        }

        #endregion Ctor

        public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
        {
            if (eventMessage?.Model is ProductDetailsModel productDetailsModel)
                await _eventTrackerService.AddEventTrackerAsync(await _eventTrackerModelFactory.PreparePutEventsRequest((await _workContext.GetCurrentCustomerAsync()).Id, productDetailsModel.Id, EventTypeEnum.View));
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (eventMessage?.Order == null)
                return;

            var order = eventMessage.Order;
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            foreach (var orderItem in orderItems)
            {
                await _eventTrackerService.AddEventTrackerAsync(await _eventTrackerModelFactory.PreparePutEventsRequest(order.CustomerId,
                    orderItem.ProductId,
                     EventTypeEnum.Purchase));
            }
        }
    }
}