﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using NopStation.Plugin.SMS.Afilnet.Domains;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace NopStation.Plugin.SMS.Afilnet.Services
{
    public partial class WorkflowSmsService : IWorkflowNotificationService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ISmsTemplateService _smsTemplateService;
        private readonly ISmsTokenProvider _afilnetsmsTokenProvider;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITokenizer _tokenizer;
        private readonly IOrderService _orderService;
        private readonly IForumService _forumService;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAclService _aclService;
        private readonly IAddressService _addressService;
        private readonly AfilnetSettings _afilnetSettings;

        #endregion

        #region Ctor

        public WorkflowSmsService(ICustomerService customerService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ISmsTemplateService smsTemplateService,
            ISmsTokenProvider afilnetsmsTokenProvider,
            IQueuedSmsService queuedSmsService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer,
            IOrderService orderService,
            IForumService forumService,
            IRepository<Customer> customerRepository,
            IGenericAttributeService genericAttributeService,
            IAclService aclService,
            IAddressService addressService,
            AfilnetSettings afilnetSettings)
        {
            _customerService = customerService;
            _languageService = languageService;
            _localizationService = localizationService;
            _smsTemplateService = smsTemplateService;
            _afilnetsmsTokenProvider = afilnetsmsTokenProvider;
            _queuedSmsService = queuedSmsService;
            _storeContext = storeContext;
            _storeService = storeService;
            _tokenizer = tokenizer;
            _orderService = orderService;
            _forumService = forumService;
            _customerRepository = customerRepository;
            _genericAttributeService = genericAttributeService;
            _aclService = aclService;
            _addressService = addressService;
            _afilnetSettings = afilnetSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task<IList<SmsTemplate>> GetActiveSmsTemplatesAsync(Customer customer, string smsTemplateName, int storeId)
        {
            //get message templates by the name
            var smsTemplates = await _smsTemplateService.GetSmsTemplatesByNameAsync(smsTemplateName, storeId);

            //no template found
            if (!smsTemplates?.Any() ?? true)
                return new List<SmsTemplate>();

            //filter active templates
            smsTemplates = smsTemplates.Where(smsTemplate => smsTemplate.Active).ToList();
            if (customer != null)
                smsTemplates = await smsTemplates.WhereAwait(async x => await _aclService.AuthorizeAsync(x, customer)).ToListAsync();

            return smsTemplates;
        }

        protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
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

        protected string GetFormattedPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            if (_afilnetSettings.CheckPhoneNumberRegex)
            {
                var match = Regex.Match(phoneNumber, _afilnetSettings.PhoneNumberRegex, RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    return null;
                }
            }

            if (_afilnetSettings.CheckIntlDialCode)
            {
                if (!phoneNumber.StartsWith(_afilnetSettings.IntlDialCode))
                {
                    if (_afilnetSettings.RemoveFirstNDigitsWhenLocalNumber > 0)
                        phoneNumber = phoneNumber[_afilnetSettings.RemoveFirstNDigitsWhenLocalNumber..];

                    phoneNumber = _afilnetSettings.IntlDialCode + phoneNumber;
                }
            }

            return phoneNumber;
        }

        protected async Task<string> GetVendorPhonenumberAsync(Vendor vendor, Customer customer, Store store)
        {
            var phoneNumber = customer.Phone;
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                var address = await _addressService.GetAddressByIdAsync(vendor.AddressId);
                phoneNumber = address?.PhoneNumber;
            }

            return GetFormattedPhoneNumber(phoneNumber);
        }

        protected string GetCustomerPhonenumber(Customer customer, string phoneNumber, Store store)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                phoneNumber = customer.Phone;

            return GetFormattedPhoneNumber(phoneNumber);
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

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.CUSTOMER_REGISTERED_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id });
            if (customers == null || !customers.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            var ids = new List<int>();
            var numbers = new List<string>();
            foreach (var admin in customers)
            {
                var phoneNumber = GetCustomerPhonenumber(admin, "", store);
                if (string.IsNullOrWhiteSpace(phoneNumber) || numbers.Contains(phoneNumber))
                    continue;

                var smsTemplates = notificationTemplates.WhereAwait(async x => await _aclService.AuthorizeAsync(x, admin));
                ids.AddRange(await smsTemplates.SelectAwait(async smsTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, admin);
                }).ToListAsync());
            }
            return ids;
        }

        public virtual async Task<IList<int>> SendCustomerWelcomeMessageAsync(Customer customer, int languageId) //can be expanded using different db table
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.CUSTOMER_WELCOME_MESSAGE, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var phoneNumber = GetCustomerPhonenumber(customer, "", store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendCustomerEmailValidationMessageAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.CUSTOMER_EMAIL_VALIDATION_MESSAGE, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var phoneNumber = GetCustomerPhonenumber(customer, "", store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
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

            var customers = _customerRepository.Table.Where(x => x.VendorId == vendor.Id);
            if (customers == null || !customers.Any())
                return new List<int>();

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(null, SmsTemplateSystemNames.ORDER_PLACED_VENDOR_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            var numbers = new List<string>();
            foreach (var customer in customers)
            {
                var vendorPhoneNumber = await GetVendorPhonenumberAsync(vendor, customer, store);
                if (string.IsNullOrWhiteSpace(vendorPhoneNumber) || numbers.Contains(vendorPhoneNumber))
                    return new List<int>();

                numbers.Add(vendorPhoneNumber);

                var smsTemplates = notificationTemplates.WhereAwait(async x => await _aclService.AuthorizeAsync(x, customer));
                ids.AddRange(await smsTemplates.SelectAwait(async smsTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(vendorPhoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
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

            var notificationTemplates = await GetActiveSmsTemplatesAsync(null, SmsTemplateSystemNames.ORDER_PLACED_ADMIN_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            var numbers = new List<string>();
            foreach (var customer in customers)
            {
                var phoneNumber = GetCustomerPhonenumber(customer, "", store);
                if (string.IsNullOrWhiteSpace(phoneNumber) || numbers.Contains(phoneNumber))
                    continue;

                var smsTemplates = notificationTemplates.WhereAwait(async x => await _aclService.AuthorizeAsync(x, customer));
                ids.AddRange(await smsTemplates.SelectAwait(async smsTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
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

            var notificationTemplates = await GetActiveSmsTemplatesAsync(null, SmsTemplateSystemNames.ORDER_PAID_ADMIN_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            var numbers = new List<string>();
            foreach (var customer in customers)
            {
                var phoneNumber = GetCustomerPhonenumber(customer, "", store);
                if (string.IsNullOrWhiteSpace(phoneNumber) || numbers.Contains(phoneNumber))
                    continue;

                var smsTemplates = notificationTemplates.WhereAwait(async x => await _aclService.AuthorizeAsync(x, customer));
                ids.AddRange(await smsTemplates.SelectAwait(async smsTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
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

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.ORDER_PAID_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var phoneNumber = GetCustomerPhonenumber(customer, billingAddress?.PhoneNumber, store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendOrderPaidVendorNotificationAsync(Order order, Vendor vendor, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var customers = _customerRepository.Table.Where(x => x.VendorId == vendor.Id);
            if (customers == null || !customers.Any())
                return new List<int>();

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(null, SmsTemplateSystemNames.ORDER_PAID_VENDOR_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            var numbers = new List<string>();
            foreach (var customer in customers)
            {
                var vendorPhoneNumber = await GetVendorPhonenumberAsync(vendor, customer, store);
                if (string.IsNullOrWhiteSpace(vendorPhoneNumber) || numbers.Contains(vendorPhoneNumber))
                    return new List<int>();

                numbers.Add(vendorPhoneNumber);

                var smsTemplates = notificationTemplates.WhereAwait(async x => await _aclService.AuthorizeAsync(x, customer));
                ids.AddRange(await smsTemplates.SelectAwait(async smsTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(vendorPhoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
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

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var phoneNumber = GetCustomerPhonenumber(customer, billingAddress?.PhoneNumber, store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
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

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.SHIPMENT_SENT_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var phoneNumber = GetCustomerPhonenumber(customer, billingAddress?.PhoneNumber, store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
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

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.SHIPMENT_DELIVERED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var phoneNumber = GetCustomerPhonenumber(customer, billingAddress?.PhoneNumber, store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendShipmentDeliveredCustomerOTPNotificationAsync(Shipment shipment, string otp, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.SHIPMENT_DELIVERED_CUSTOMER_OTP_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var phoneNumber = GetCustomerPhonenumber(customer, billingAddress?.PhoneNumber, store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            _afilnetsmsTokenProvider.AddOTPTokens(commonTokens, otp);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendOrderCompletedCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.ORDER_COMPLETED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var phoneNumber = GetCustomerPhonenumber(customer, billingAddress?.PhoneNumber, store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendOrderCancelledCustomerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.ORDER_CANCELLED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var phoneNumber = GetCustomerPhonenumber(customer, billingAddress?.PhoneNumber, store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
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

            var notificationTemplates = await GetActiveSmsTemplatesAsync(null, SmsTemplateSystemNames.ORDER_REFUNDED_ADMIN_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddOrderRefundedTokensAsync(commonTokens, order, refundedAmount);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, await _customerService.GetCustomerByIdAsync(order.CustomerId));

            var ids = new List<int>();
            var numbers = new List<string>();
            foreach (var customer in customers)
            {
                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                var phoneNumber = GetCustomerPhonenumber(customer, billingAddress?.PhoneNumber, store);
                if (string.IsNullOrWhiteSpace(phoneNumber) || numbers.Contains(phoneNumber))
                    continue;

                var smsTemplates = notificationTemplates.WhereAwait(async x => await _aclService.AuthorizeAsync(x, customer));
                ids.AddRange(await smsTemplates.SelectAwait(async smsTemplate =>
                {
                    var tokens = new List<Token>(commonTokens);
                    await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                    return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
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

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.ORDER_REFUNDED_CUSTOMER_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var phoneNumber = GetCustomerPhonenumber(customer, billingAddress?.PhoneNumber, store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _afilnetsmsTokenProvider.AddOrderRefundedTokensAsync(commonTokens, order, refundedAmount);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
            }).ToListAsync();
        }

        #endregion

        #region Forum Notifications

        public virtual async Task<IList<int>> SendNewForumTopicMessageAsync(Customer customer, ForumTopic forumTopic, Forum forum, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.NEW_FORUM_TOPIC_MESSAGE, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var phoneNumber = GetCustomerPhonenumber(customer, "", store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            var forums = await _forumService.GetForumByIdAsync(forumTopic.ForumId);
            if (forums == null)
                throw new ArgumentException("forum cannot be loaded");

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddForumTopicTokensAsync(commonTokens, forumTopic);
            await _afilnetsmsTokenProvider.AddForumTokensAsync(commonTokens, forums);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendNewForumPostMessageAsync(Customer customer, ForumPost forumPost, ForumTopic forumTopic,
            Forum forum, int friendlyForumTopicPageIndex, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.NEW_FORUM_POST_MESSAGE, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var phoneNumber = GetCustomerPhonenumber(customer, "", store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
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
            await _afilnetsmsTokenProvider.AddForumPostTokensAsync(commonTokens, forumPost);
            await _afilnetsmsTokenProvider.AddForumTopicTokensAsync(commonTokens, forumTopics, friendlyForumTopicPageIndex, forumPost.Id);
            await _afilnetsmsTokenProvider.AddForumTokensAsync(commonTokens, forums);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendPrivateMessageNotificationAsync(PrivateMessage privateMessage, int languageId)
        {
            if (privateMessage == null)
                throw new ArgumentNullException(nameof(privateMessage));

            var store = await _storeService.GetStoreByIdAsync(privateMessage.StoreId) ?? await _storeContext.GetCurrentStoreAsync();

            var customer = await _customerService.GetCustomerByIdAsync(privateMessage.ToCustomerId);

            var notificationTemplates = await GetActiveSmsTemplatesAsync(customer, SmsTemplateSystemNames.PRIVATE_MESSAGE_NOTIFICATION, store.Id);
            if (!notificationTemplates.Any())
                return new List<int>();

            var phoneNumber = GetCustomerPhonenumber(customer, "", store);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _afilnetsmsTokenProvider.AddPrivateMessageTokensAsync(commonTokens, privateMessage);
            await _afilnetsmsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await notificationTemplates.SelectAwait(async smsTemplate =>
            {
                var tokens = new List<Token>(commonTokens);
                await _afilnetsmsTokenProvider.AddStoreTokensAsync(tokens, store);

                return await SendNotificationAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
            }).ToListAsync();
        }

        #endregion

        #region Misc

        public virtual async Task<int> SendNotificationAsync(string phoneNumber, SmsTemplate smsTemplate,
            int languageId, IEnumerable<Token> tokens, int storeId, Customer customer)
        {
            if (smsTemplate == null)
                throw new ArgumentNullException(nameof(smsTemplate));

            var language = await _languageService.GetLanguageByIdAsync(languageId);

            var body = await _localizationService.GetLocalizedAsync(smsTemplate, mt => mt.Body, languageId);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            return await SendNotificationAsync(phoneNumber, bodyReplaced, storeId, customer);
        }

        public async Task<int> SendNotificationAsync(string phoneNumber, string body, int storeId, Customer customer)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return 0;

            if (customer == null)
                return 0;

            var queuedSms = new QueuedSms
            {
                Body = body,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customer.Id,
                StoreId = storeId,
                PhoneNumber = phoneNumber
            };

            await _queuedSmsService.InsertQueuedSmsAsync(queuedSms);
            return queuedSms.Id;
        }

        #endregion

        #endregion
    }
}