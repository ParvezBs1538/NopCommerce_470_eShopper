//using System.Threading.Tasks;
//using Nop.Core.Domain.Orders;
//using Nop.Core.Events;
//using NopStation.Plugin.Shipping.Redx.Domains;
//using Nop.Services.Events;

//namespace NopStation.Plugin.Shipping.Redx.Services
//{
//    public class OrderEventConsumer : IConsumer<OrderPlacedEvent>, IConsumer<EntityDeletedEvent<Order>>, IConsumer<EntityUpdatedEvent<Order>>
//    {
//        private readonly IRedxShipmentService _redxShipmentService;

//        #region ctor

//        public OrderEventConsumer(IRedxShipmentService redxShipmentService)
//        {
//            _redxShipmentService = redxShipmentService;
//        }

//        #endregion ctor

//        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
//        {
//            if (eventMessage.Order == null)
//                return;
//            var redxChecker = eventMessage.Order.ShippingRateComputationMethodSystemName.Equals("NopStation.Redx");
//            if (redxChecker)
//            {
//                var alreadyAddedShipment = await _redxShipmentService.GetRedxShipmentByOrderIdAsync(eventMessage.Order.Id);
//                if (alreadyAddedShipment == null)
//                {
//                    var shipment = new RedxShipment
//                    {
//                        OrderId = eventMessage.Order.Id
//                    };
//                    await _redxShipmentService.InsertShipmentAsync(shipment);
//                }
//            }
//        }

//        public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
//        {
//            if (eventMessage.Entity == null)
//                return;
//            var redxShipment = await _redxShipmentService.GetRedxShipmentByIdAsync(eventMessage.Entity.Id);
//            if (redxShipment != null)
//            {
//                await _redxShipmentService.DeleteShipmentAsync(redxShipment);
//            }
//        }

//        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
//        {
//            if (eventMessage.Entity.ShippingRateComputationMethodSystemName.Equals("NopStation.Redx"))
//            {
//                var ifAlreadyAddedShipment = await _redxShipmentService.GetRedxShipmentByOrderIdAsync(eventMessage.Entity.Id);
//                if (ifAlreadyAddedShipment == null)
//                {
//                    var shipment = new RedxShipment
//                    {
//                        OrderId = eventMessage.Entity.Id
//                    };
//                   await _redxShipmentService.InsertShipmentAsync(shipment);

//                }

//            }
//            else
//            {
//                var alreadyECourierChoosenShipment = await _redxShipmentService.GetRedxShipmentByOrderIdAsync(eventMessage.Entity.Id);
//                if (alreadyECourierChoosenShipment != null || alreadyECourierChoosenShipment?.OrderId == eventMessage.Entity.Id)
//                {
//                    await _redxShipmentService.DeleteShipmentAsync(alreadyECourierChoosenShipment);
//                }
//            }
//        }
//    }
//}