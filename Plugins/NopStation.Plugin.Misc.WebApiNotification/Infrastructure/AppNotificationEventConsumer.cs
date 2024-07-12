using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification.Infrastructure
{
    public class AppNotificationEventConsumer : IConsumer<CustomerRegisteredEvent>,
        IConsumer<OrderPlacedEvent>,
        IConsumer<OrderPaidEvent>,
        IConsumer<OrderStatusChangedEvent>,
        IConsumer<ShipmentSentEvent>,
        IConsumer<ShipmentDeliveredEvent>,
        IConsumer<OrderRefundedEvent>
    {
        private readonly IWorkContext _workContext;
        private readonly IWorkflowNotificationService _workflowNotificationService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        public AppNotificationEventConsumer(IWorkContext workContext,
            IWorkflowNotificationService workflowNotificationService,
            CustomerSettings customerSettings,
            ICustomerService customerService,
            IOrderService orderService)
        {
            _workContext = workContext;
            _workflowNotificationService = workflowNotificationService;
            _customerSettings = customerSettings;
            _customerService = customerService;
            _orderService = orderService;
        }

        public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            if (eventMessage.Customer == null)
                return;

            var languae = await _workContext.GetWorkingLanguageAsync();
            if (await _customerService.IsAdminAsync(eventMessage.Customer))
                await _workflowNotificationService.SendCustomerRegisteredNotificationAsync(eventMessage.Customer, languae.Id);

            switch (_customerSettings.UserRegistrationType)
            {
                case UserRegistrationType.EmailValidation:
                    await _workflowNotificationService.SendCustomerEmailValidationNotificationAsync(eventMessage.Customer, languae.Id);
                    break;
                case UserRegistrationType.Standard:
                    await _workflowNotificationService.SendCustomerCustomerRegisteredWelcomeNotificationAsync(eventMessage.Customer, languae.Id);
                    break;
                default:
                    break;
            }
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            var order = eventMessage.Order;
            var languageId = order.CustomerLanguageId;

            await _workflowNotificationService.SendOrderPlacedCustomerNotificationAsync(order, languageId);
        }

        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            var order = eventMessage.Order;
            var languageId = order.CustomerLanguageId;

            await _workflowNotificationService.SendOrderPaidCustomerNotificationAsync(order, languageId);
        }

        public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            var order = eventMessage.Order;
            var languageId = order.CustomerLanguageId;

            if (order.OrderStatus != OrderStatus.Cancelled && order.OrderStatus != OrderStatus.Complete)
                return;

            if (order.OrderStatus == OrderStatus.Complete)
                await _workflowNotificationService.SendOrderCompletedCustomerNotificationAsync(order, order.CustomerLanguageId);
            else
                await _workflowNotificationService.SendOrderCancelledCustomerNotificationAsync(order, languageId);
        }

        public async Task HandleEventAsync(ShipmentSentEvent eventMessage)
        {
            if (eventMessage.Shipment == null)
                return;

            var order = await _orderService.GetOrderByIdAsync(eventMessage.Shipment.OrderId);
            await _workflowNotificationService.SendShipmentSentCustomerNotificationAsync(eventMessage.Shipment, order.CustomerLanguageId);
        }

        public async Task HandleEventAsync(ShipmentDeliveredEvent eventMessage)
        {
            if (eventMessage.Shipment == null)
                return;

            var order = await _orderService.GetOrderByIdAsync(eventMessage.Shipment.OrderId);
            await _workflowNotificationService.SendShipmentDeliveredCustomerNotificationAsync(eventMessage.Shipment, order.CustomerLanguageId);
        }

        public async Task HandleEventAsync(OrderRefundedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            var order = eventMessage.Order;
            var languageId = order.CustomerLanguageId;

            await _workflowNotificationService.SendOrderRefundedCustomerNotificationAsync(order,
                eventMessage.Amount, languageId);
        }
    }
}
