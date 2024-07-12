using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Services.Customers;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public partial class WorkflowNotificationService : IWorkflowNotificationService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPushNotificationCustomerService _pushNotificationCustomerService;
        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;
        private readonly IPushNotificationTokenProvider _pushNotificationTokenProvider;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITokenizer _tokenizer;
        private readonly IPictureService _pictureService;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;
        private readonly IOrderService _orderService;
        private readonly IForumService _forumService;

        #endregion

        #region Ctor

        public WorkflowNotificationService(ICustomerService customerService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IPushNotificationCustomerService pushNotificationCustomerService,
            IPushNotificationTemplateService pushNotificationTemplateService,
            IPushNotificationTokenProvider pushNotificationTokenProvider,
            IQueuedPushNotificationService queuedPushNotificationService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer,
            IPictureService pictureService,
            ProgressiveWebAppSettings progressiveWebAppSettings,
            IOrderService orderService,
            IForumService forumService)
        {
            _customerService = customerService;
            _languageService = languageService;
            _localizationService = localizationService;
            _pushNotificationCustomerService = pushNotificationCustomerService;
            _pushNotificationTemplateService = pushNotificationTemplateService;
            _pushNotificationTokenProvider = pushNotificationTokenProvider;
            _queuedPushNotificationService = queuedPushNotificationService;
            _storeContext = storeContext;
            _storeService = storeService;
            _tokenizer = tokenizer;
            _pictureService = pictureService;
            _progressiveWebAppSettings = progressiveWebAppSettings;
            _orderService = orderService;
            _forumService = forumService;
        }

        #endregion

        #region Utilities

        public virtual async Task<IList<PushNotificationTemplate>> GetActivePushNotificationTemplatesAsync(string pushNotificationTemplateName, int storeId)
        {
            //get message templates by the name
            var pushNotificationTemplates = await _pushNotificationTemplateService.GetPushNotificationTemplatesByNameAsync(pushNotificationTemplateName, storeId);

            //no template found
            if (!pushNotificationTemplates?.Any() ?? true)
                return new List<PushNotificationTemplate>();

            //filter active templates
            pushNotificationTemplates = pushNotificationTemplates.Where(pushNotificationTemplate => pushNotificationTemplate.Active).ToList();

            return pushNotificationTemplates;
        }

        public virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
        {
            //load language by specified ID
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");

            return language.Id;
        }

        #endregion

        #region Methods

        #region Customer workflow

        public virtual async Task<IList<int>> SendCustomerRegisteredNotificationMessageAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.CustomerRegisteredNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendCustomerWelcomeMessageAsync(Customer customer, int languageId) //can be expanded using different db table
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.CustomerWelcomeMessage, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendCustomerEmailValidationMessageAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.CustomerEmailValidationMessage, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
            }).ToListAsync();
        }

        #endregion

        #region Order workflow

        public virtual async Task<IList<int>> SendOrderPlacedVendorNotificationAsync(Order order, Vendor vendor, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var customers = _pushNotificationCustomerService.GetCustomersByVendorId(vendor.Id);
            if (customers == null || !customers.Any())
                return new List<int>();

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderPlacedVendorNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var customer in customers)
            {
                ids.AddRange(await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);

                }).ToListAsync());
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendOrderPlacedAdminNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id });
            if (customers == null || !customers.Any())
                return new List<int>();

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderPlacedAdminNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //var customer = _customerService.GetCustomerById(order.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var customer in customers)
            {
                ids.AddRange(await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
                }).ToListAsync());
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendOrderPaidAdminNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id });
            if (customers == null || !customers.Any())
                return new List<int>();

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderPaidAdminNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var customer in customers)
            {
                ids.AddRange(await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
                }).ToListAsync());
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendOrderPaidCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderPaidCustomerNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, order.CustomerId);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendOrderPaidVendorNotificationAsync(Order order, Vendor vendor, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var customers = _pushNotificationCustomerService.GetCustomersByVendorId(vendor.Id);
            if (customers == null || !customers.Any())
                return new List<int>();

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderPaidVendorNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var customer in customers)
            {
                ids.AddRange(await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
                }).ToListAsync());
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendOrderPlacedCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderPlacedCustomerNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, order.CustomerId);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendShipmentSentCustomerNotificationAsync(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.ShipmentSentCustomerNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendShipmentDeliveredCustomerNotificationAsync(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.ShipmentDeliveredCustomerNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendOrderCompletedCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderCompletedCustomerNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, order.CustomerId);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendOrderCancelledCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderCancelledCustomerNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, order.CustomerId);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendOrderRefundedAdminNotificationAsync(Order order, decimal refundedAmount, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id });
            if (customers == null || !customers.Any())
                return new List<int>();

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderRefundedAdminNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();


            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddOrderRefundedTokensAsync(commonTokens, order, refundedAmount);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var customer in customers)
            {
                ids.AddRange(await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
                }).ToListAsync());
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendOrderRefundedCustomerNotificationAsync(Order order, decimal refundedAmount, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.OrderRefundedCustomerNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddOrderRefundedTokensAsync(commonTokens, order, refundedAmount);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, order.CustomerId);
            }).ToListAsync();
        }

        #endregion

        #region Forum Notifications

        public virtual async Task<IList<int>> SendNewForumTopicMessageAsync(Customer customer, ForumTopic forumTopic, Forum forum, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.NewForumTopicMessage, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var forums = await _forumService.GetForumByIdAsync(forumTopic.ForumId);
            if (forums == null)
                throw new ArgumentException("forum cannot be loaded");

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddForumTopicTokensAsync(commonTokens, forumTopic);
            await _pushNotificationTokenProvider.AddForumTokensAsync(commonTokens, forums);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
           {
               var tokens = new List<Token>(commonTokens);
               await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

               return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
           }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendNewForumPostMessageAsync(Customer customer, ForumPost forumPost, ForumTopic forumTopic,
            Forum forum, int friendlyForumTopicPageIndex, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.NewForumPostMessage, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            // Here forum is correctly pulled. 

            var forumTopics = await _forumService.GetTopicByIdAsync(forumPost.TopicId);
            if (forumTopics == null)
                throw new ArgumentException("forum topic cannot be loaded");

            var forums = await _forumService.GetForumByIdAsync(forumTopics.ForumId);
            if (forums == null)
                throw new ArgumentException("forum cannot be loaded");

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddForumPostTokensAsync(commonTokens, forumPost);
            await _pushNotificationTokenProvider.AddForumTopicTokensAsync(commonTokens, forumTopics, friendlyForumTopicPageIndex, forumPost.Id);
            await _pushNotificationTokenProvider.AddForumTokensAsync(commonTokens, forums);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendPrivateMessageNotificationAsync(PrivateMessage privateMessage, int languageId)
        {
            if (privateMessage == null)
                throw new ArgumentNullException(nameof(privateMessage));

            var store = await _storeService.GetStoreByIdAsync(privateMessage.StoreId) ?? await _storeContext.GetCurrentStoreAsync();

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.PrivateMessageNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(privateMessage.ToCustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddPrivateMessageTokensAsync(commonTokens, privateMessage);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, privateMessage.ToCustomerId);
            }).ToListAsync();
        }

        #endregion

        #region Misc

        public virtual async Task<IList<int>> SendAbandonedCartNotificationAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(PushNotificationTemplateSystemNames.AbandonedCartNotification, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async pushNotificationTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(pushNotificationTemplate, languageId, tokens, store.Id, customer.Id);
            }).ToListAsync();
        }

        public virtual async Task<int> SendNotificationAsync(PushNotificationTemplate pushNotificationTemplate,
            int languageId, IEnumerable<Token> tokens, int storeId, int customerId)
        {
            if (pushNotificationTemplate == null)
                throw new ArgumentNullException(nameof(pushNotificationTemplate));

            var language = await _languageService.GetLanguageByIdAsync(languageId);

            var title = await _localizationService.GetLocalizedAsync(pushNotificationTemplate, mt => mt.Title, languageId);
            var titleReplaced = _tokenizer.Replace(title, tokens, true);

            var body = await _localizationService.GetLocalizedAsync(pushNotificationTemplate, mt => mt.Body, languageId);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            var url = pushNotificationTemplate.Url;
            var urlReplaced = !string.IsNullOrWhiteSpace(url) ? _tokenizer.Replace(url, tokens, true) : null;

            var iconUrl = "";
            if (pushNotificationTemplate.UseDefaultIcon)
                iconUrl = await _pictureService.GetPictureUrlAsync(_progressiveWebAppSettings.DefaultIconId, 80);
            else
                iconUrl = await _pictureService.GetPictureUrlAsync(pushNotificationTemplate.IconId, 80);

            var imageUrl = await _pictureService.GetPictureUrlAsync(pushNotificationTemplate.ImageId, showDefaultPicture: false);
            if (string.IsNullOrWhiteSpace(imageUrl))
                imageUrl = null;

            return SendNotification(titleReplaced, bodyReplaced, iconUrl, imageUrl, urlReplaced, storeId, customerId, language.Rtl);
        }

        public int SendNotification(string title, string body, string iconUrl, string imageUrl,
            string url, int storeId, int customerId, bool rtl)
        {
            var queuedPushNotification = new QueuedPushNotification
            {
                Body = body,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customerId,
                IconUrl = iconUrl,
                StoreId = storeId,
                Title = title,
                ImageUrl = imageUrl,
                Url = url,
                Rtl = rtl
            };

            _queuedPushNotificationService.InsertQueuedPushNotificationAsync(queuedPushNotification);
            return queuedPushNotification.Id;
        }

        #endregion

        #endregion
    }
}