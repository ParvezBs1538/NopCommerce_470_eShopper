using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using NopStation.Plugin.SMS.BulkSms.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Forums;
using Nop.Services.Orders;
using Nop.Services.Vendors;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.BulkSms.Infrastructure
{
    public class SmsEventConsumer : IConsumer<CustomerRegisteredEvent>,
        IConsumer<OrderPlacedEvent>,
        IConsumer<OrderPaidEvent>,
        IConsumer<ShipmentSentEvent>,
        IConsumer<ShipmentDeliveredEvent>,
        IConsumer<OrderRefundedEvent>,
        IConsumer<EntityInsertedEvent<ForumTopic>>,
        IConsumer<EntityInsertedEvent<ForumPost>>,
        IConsumer<EntityInsertedEvent<PrivateMessage>>
    {
        private readonly IWorkContext _workContext;
        private readonly BulkSmsSettings _bulkSmsSettings;
        private readonly IWorkflowNotificationService _workflowNotificationService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IVendorService _vendorService;
        private readonly ICustomerService _customerService;
        private readonly IForumService _forumService;
        private readonly ForumSettings _forumSettings;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public SmsEventConsumer(IWorkContext workContext,
            BulkSmsSettings bulkSmsSettings,
            IWorkflowNotificationService workflowNotificationService,
            LocalizationSettings localizationSettings,
            CustomerSettings customerSettings,
            IVendorService vendorService,
            ICustomerService customerService,
            IForumService forumService,
            ForumSettings forumSettings,
            IOrderService orderService, 
            IProductService productService)
        {
            _workContext = workContext;
            _bulkSmsSettings = bulkSmsSettings;
            _workflowNotificationService = workflowNotificationService;
            _localizationSettings = localizationSettings;
            _customerSettings = customerSettings;
            _vendorService = vendorService;
            _customerService = customerService;
            _forumService = forumService;
            _forumSettings = forumSettings;
            _orderService = orderService;
            _productService = productService;
        }

        #region Utilities

        protected virtual async Task<IList<Vendor>> GetVendorsInOrderAsync(Order order)
        {
            var vendors = new List<Vendor>();
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                var vendorId = product.VendorId;
                //find existing
                var vendor = vendors.FirstOrDefault(v => v.Id == vendorId);
                if (vendor != null)
                    continue;

                //not found. load by Id
                vendor = await _vendorService.GetVendorByIdAsync(vendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    vendors.Add(vendor);
                }
            }

            return vendors;
        }

        #endregion

        public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            if (!_bulkSmsSettings.EnablePlugin)
                return;

            await _workflowNotificationService.SendCustomerRegisteredNotificationMessageAsync(eventMessage.Customer, _localizationSettings.DefaultAdminLanguageId);

            switch (_customerSettings.UserRegistrationType)
            {
                case UserRegistrationType.EmailValidation:
                    await _workflowNotificationService.SendCustomerEmailValidationMessageAsync(eventMessage.Customer,
                        (await _workContext.GetWorkingLanguageAsync()).Id);
                    break;
                case UserRegistrationType.Standard:
                    await _workflowNotificationService.SendCustomerWelcomeMessageAsync(eventMessage.Customer,
                        (await _workContext.GetWorkingLanguageAsync()).Id);
                    break;
                default:
                    break;
            }
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (!_bulkSmsSettings.EnablePlugin)
                return;

            var vendors = await GetVendorsInOrderAsync(eventMessage.Order);
            foreach (var vendor in vendors)
                await _workflowNotificationService.SendOrderPlacedVendorNotificationAsync(eventMessage.Order, vendor, _localizationSettings.DefaultAdminLanguageId);

            await _workflowNotificationService.SendOrderPlacedCustomerNotificationAsync(eventMessage.Order,
                     _localizationSettings.DefaultAdminLanguageId);
            await _workflowNotificationService.SendOrderPlacedAdminNotificationAsync(eventMessage.Order,
                     _localizationSettings.DefaultAdminLanguageId);
        }

        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            if (!_bulkSmsSettings.EnablePlugin)
                return;

            var vendors = await GetVendorsInOrderAsync(eventMessage.Order);
            foreach (var vendor in vendors)
                await _workflowNotificationService.SendOrderPaidVendorNotificationAsync(eventMessage.Order, vendor, _localizationSettings.DefaultAdminLanguageId);

            await _workflowNotificationService.SendOrderPaidCustomerNotificationAsync(eventMessage.Order,
                     _localizationSettings.DefaultAdminLanguageId);
            await _workflowNotificationService.SendOrderPaidAdminNotificationAsync(eventMessage.Order,
                     _localizationSettings.DefaultAdminLanguageId);
        }

        public async Task HandleEventAsync(ShipmentSentEvent eventMessage)
        {
            if (!_bulkSmsSettings.EnablePlugin)
                return;

            await _workflowNotificationService.SendShipmentSentCustomerNotificationAsync(eventMessage.Shipment,
                (await _orderService.GetOrderByIdAsync(eventMessage.Shipment.OrderId)).CustomerLanguageId);
        }

        public async Task HandleEventAsync(ShipmentDeliveredEvent eventMessage)
        {
            if (!_bulkSmsSettings.EnablePlugin)
                return;

            await _workflowNotificationService.SendShipmentDeliveredCustomerNotificationAsync(eventMessage.Shipment,
                (await _orderService.GetOrderByIdAsync(eventMessage.Shipment.OrderId)).CustomerLanguageId);
        }

        public async Task HandleEventAsync(OrderRefundedEvent eventMessage)
        {
            if (!_bulkSmsSettings.EnablePlugin)
                return;

            await _workflowNotificationService.SendOrderRefundedAdminNotificationAsync(eventMessage.Order,
                eventMessage.Amount, eventMessage.Order.CustomerLanguageId);
            await _workflowNotificationService.SendOrderRefundedCustomerNotificationAsync(eventMessage.Order,
                eventMessage.Amount, eventMessage.Order.CustomerLanguageId);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ForumTopic> eventMessage)
        {
            if (!_bulkSmsSettings.EnablePlugin)
                return;

            var subscriptions = await _forumService.GetAllSubscriptionsAsync(forumId: eventMessage.Entity.ForumId);
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            foreach (var subscription in subscriptions)
            {
                if (subscription.CustomerId == eventMessage.Entity.CustomerId)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty((await _customerService.GetCustomerByIdAsync(subscription.CustomerId)).Email))
                {
                    var forum = await _forumService.GetForumByIdAsync(eventMessage.Entity.ForumId);

                    await _workflowNotificationService.SendNewForumTopicMessageAsync(await _customerService.GetCustomerByIdAsync(subscription.CustomerId),
                        eventMessage.Entity, forum, languageId);
                }
            }
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ForumPost> eventMessage)
        {
            if (!_bulkSmsSettings.EnablePlugin)
                return;

            var forumTopic = await _forumService.GetTopicByIdAsync(eventMessage.Entity.TopicId);
            var forum = await _forumService.GetForumByIdAsync(forumTopic.ForumId);

            var subscriptions = await _forumService.GetAllSubscriptionsAsync(topicId: forumTopic.Id);
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            var friendlyTopicPageIndex = await _forumService.CalculateTopicPageIndexAsync(eventMessage.Entity.TopicId,
                                             _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10,
                                             eventMessage.Entity.Id) + 1;

            foreach (var subscription in subscriptions)
            {
                if (subscription.CustomerId == eventMessage.Entity.CustomerId)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty((await _customerService.GetCustomerByIdAsync(subscription.CustomerId)).Email))
                {
                    await _workflowNotificationService.SendNewForumPostMessageAsync(await _customerService.GetCustomerByIdAsync(subscription.CustomerId), eventMessage.Entity,
                        forumTopic, forum, friendlyTopicPageIndex, languageId);
                }
            }
        }

        public async Task HandleEventAsync(EntityInsertedEvent<PrivateMessage> eventMessage)
        {
            if (!_bulkSmsSettings.EnablePlugin)
                return;

            await _workflowNotificationService.SendPrivateMessageNotificationAsync(eventMessage.Entity, (await _workContext.GetWorkingLanguageAsync()).Id);
        }
    }
}
