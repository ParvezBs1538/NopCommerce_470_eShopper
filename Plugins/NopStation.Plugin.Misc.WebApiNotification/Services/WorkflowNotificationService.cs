using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Services;
using NopStation.Plugin.Misc.WebApiNotification.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Extensions;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public partial class WorkflowNotificationService : IWorkflowNotificationService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;
        private readonly IPushNotificationTokenProvider _pushNotificationTokenProvider;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITokenizer _tokenizer;
        private readonly IPictureService _pictureService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IApiDeviceService _apiDeviceService;
        private readonly IPushNotificationCampaignService _pushNotificationCampaignService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public WorkflowNotificationService(ICustomerService customerService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IPushNotificationTemplateService pushNotificationTemplateService,
            IPushNotificationTokenProvider pushNotificationTokenProvider,
            IQueuedPushNotificationService queuedPushNotificationService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer,
            IPictureService pictureService,
            IGenericAttributeService genericAttributeService,
            IApiDeviceService apiDeviceService,
            IPushNotificationCampaignService pushNotificationCampaignService,
            IOrderService orderService)
        {
            _customerService = customerService;
            _languageService = languageService;
            _localizationService = localizationService;
            _pushNotificationTemplateService = pushNotificationTemplateService;
            _pushNotificationTokenProvider = pushNotificationTokenProvider;
            _queuedPushNotificationService = queuedPushNotificationService;
            _storeContext = storeContext;
            _storeService = storeService;
            _tokenizer = tokenizer;
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _apiDeviceService = apiDeviceService;
            _pushNotificationCampaignService = pushNotificationCampaignService;
            _orderService = orderService;
        }

        #endregion

        #region Utilities

        protected virtual async Task<IList<WebApiNotificationTemplate>> GetActivePushNotificationTemplatesAsync(string notificationTemplateName, int storeId)
        {
            //get message templates by the name
            var pushNotificationTemplates = await _pushNotificationTemplateService.GetPushNotificationTemplatesByNameAsync(notificationTemplateName, storeId);

            //no template found
            if (!pushNotificationTemplates?.Any() ?? true)
                return new List<WebApiNotificationTemplate>();

            //filter active templates
            return pushNotificationTemplates.Where(notificationTemplate => notificationTemplate.Active).ToList();
        }

        protected virtual async Task<Language> GetDefaultLanguage(int storeId = 0)
        {
            //load any language from the specified store
            var language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).OrderBy(l => l.DisplayOrder).FirstOrDefault(l => l.Published);

            if (language == null)
            {
                //load any language
                language = (await _languageService.GetAllLanguagesAsync()).OrderBy(l => l.DisplayOrder).FirstOrDefault();

                if (language == null)
                    throw new Exception("No active language could be loaded");
            }

            return language;
        }

        protected virtual async Task<int> EnsureLanguageIsActive(int languageId = 0, int storeId = 0)
        {
            if (languageId == 0)
                return (await GetDefaultLanguage(storeId: storeId)).Id;

            //load language by specified ID
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language == null || !language.Published)
            {
                //load any language
                language = await GetDefaultLanguage(storeId);
            }

            return language.Id;
        }

        #endregion

        #region Methods

        #region Campaigns

        public virtual async Task<IList<int>> SendCampaignNotificationAsync(WebApiNotificationCampaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            var pageIndex = 0;
            var ids = new List<int>();

            var store = await _storeService.GetStoreByIdAsync(campaign.LimitedToStoreId);
            if (store == null)
                store = await _storeContext.GetCurrentStoreAsync();

            while (true)
            {
                var devices = await _pushNotificationCampaignService.GetCampaignDevicesAsync(campaign, pageIndex++, 100);
                if (!devices.Any())
                    break;

                foreach (var device in devices)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(device.CustomerId);
                    var languageId = customer?.LanguageId ?? 0;
                    languageId = await EnsureLanguageIsActive(languageId, store.Id);

                    //tokens
                    var commonTokens = new List<Token>();
                    if (customer != null)
                        await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

                    var tokens = new List<Token>(commonTokens);
                    await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                    var ds = new List<ApiDevice>() { device };
                    ids.AddRange(await SendNotificationAsync(ds, campaign, languageId, tokens, store.Id));
                }
            }
            return ids;
        }

        #endregion

        #region Customer workflow

        public virtual async Task<IList<int>> SendCustomerRegisteredNotificationAsync(Customer customer, int languageId = 0)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.CUSTOMER_REGISTERED_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(customer, notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendCustomerCustomerRegisteredWelcomeNotificationAsync(Customer customer, int languageId) //can be expanded using different db table
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.CUSTOMER_REGISTERED_WELCOME_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(customer, notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendCustomerEmailValidationNotificationAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.CUSTOMER_EMAIL_VALIDATION_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(customer, notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendCustomerCustomerWelcomeNotificationAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.CUSTOMER_WELCOME_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(customer, notificationTemplate, languageId, tokens, store.Id));
            }

            return ids;
        }

        #endregion

        #region Order workflow

        public virtual async Task<IList<int>> SendOrderPaidCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.ORDER_PAID_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(await _customerService.GetCustomerByIdAsync(order.CustomerId), notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendOrderPlacedCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(await _customerService.GetCustomerByIdAsync(order.CustomerId), notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendShipmentSentCustomerNotificationAsync(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.SHIPMENT_SENT_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(await _customerService.GetCustomerByIdAsync(order.CustomerId), notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendShipmentDeliveredCustomerNotificationAsync(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.SHIPMENT_DELIVERED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(await _customerService.GetCustomerByIdAsync(order.CustomerId), notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendOrderCompletedCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.ORDER_COMPLETED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(await _customerService.GetCustomerByIdAsync(order.CustomerId), notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendOrderCancelledCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.ORDER_CANCELLED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(await _customerService.GetCustomerByIdAsync(order.CustomerId), notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendOrderRefundedCustomerNotificationAsync(Order order, decimal refundedAmount, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActive(languageId, store.Id);

            var notificationTemplates = await GetActivePushNotificationTemplatesAsync(WebApiNotificationTemplateSystemNames.ORDER_REFUNDED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _pushNotificationTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _pushNotificationTokenProvider.AddOrderRefundedTokensAsync(commonTokens, order, refundedAmount);
            await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            foreach (var notificationTemplate in notificationTemplates)
            {
                var tokens = new List<Token>(commonTokens);
                await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);
                ids.AddRange(await SendNotificationAsync(await _customerService.GetCustomerByIdAsync(order.CustomerId), notificationTemplate, languageId, tokens, store.Id));
            }
            return ids;
        }

        #endregion

        #region Misc

        public virtual async Task<IList<int>> SendNotificationAsync(Customer customer,
            WebApiNotificationTemplate template, int languageId, IEnumerable<Token> tokens, int storeId)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            var language = await _languageService.GetLanguageByIdAsync(languageId);

            var title = await _localizationService.GetLocalizedAsync(template, mt => mt.Title, languageId);
            var titleReplaced = _tokenizer.Replace(title, tokens, true);

            var body = await _localizationService.GetLocalizedAsync(template, mt => mt.Body, languageId);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            var valueReplaced = !string.IsNullOrWhiteSpace(template.ActionValue) ?
                _tokenizer.Replace(template.ActionValue, tokens, true) : null;

            var imageUrl = await _pictureService.GetPictureUrlAsync(template.ImageId, showDefaultPicture: false);
            if (string.IsNullOrWhiteSpace(imageUrl))
                imageUrl = null;

            var sendTime = template.SendImmediately || !template.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromMinutes(template.DelayPeriod.ToMinutes(template.DelayBeforeSend.Value)));

            var devices = (await _apiDeviceService.SearchApiDevicesAsync(customer.Id)).DistinctBy(ad => ad.DeviceToken).ToList();
            return await SendNotificationAsync(devices, titleReplaced, bodyReplaced, imageUrl,
                template.ActionType, valueReplaced, storeId, sendTime);
        }

        public virtual async Task<IList<int>> SendNotificationAsync(IList<ApiDevice> devices,
            WebApiNotificationCampaign campaign, int languageId, IEnumerable<Token> tokens, int storeId)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            var language = await _languageService.GetLanguageByIdAsync(languageId);

            var title = await _localizationService.GetLocalizedAsync(campaign, mt => mt.Title, languageId);
            var titleReplaced = _tokenizer.Replace(title, tokens, true);

            var body = await _localizationService.GetLocalizedAsync(campaign, mt => mt.Body, languageId);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            var imageUrl = await _pictureService.GetPictureUrlAsync(campaign.ImageId, showDefaultPicture: false);
            if (string.IsNullOrWhiteSpace(imageUrl))
                imageUrl = null;

            return await SendNotificationAsync(devices, titleReplaced, bodyReplaced, imageUrl, campaign.ActionType,
                campaign.ActionValue, storeId);
        }

        public async Task<IList<int>> SendNotificationAsync(IList<ApiDevice> devices, string title, string body,
            string imageUrl, NotificationActionType actionType, string actionValue,
            int storeId, DateTime? sendTime = null)
        {
            var ids = new List<int>();
            foreach (var device in devices)
            {
                var queuedPushNotification = new WebApiQueuedNotification
                {
                    Body = body,
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = device.CustomerId,
                    StoreId = storeId,
                    Title = title,
                    ImageUrl = imageUrl,
                    DontSendBeforeDateUtc = sendTime,
                    AppDeviceId = device.Id,
                    ActionValue = actionValue,
                    ActionType = actionType,
                    DeviceTypeId = device.DeviceTypeId,
                    SubscriptionId = device.SubscriptionId
                };

                await _queuedPushNotificationService.InsertQueuedPushNotificationAsync(queuedPushNotification);
                ids.Add(queuedPushNotification.Id);
            }

            return ids;
        }

        #endregion

        #endregion
    }
}