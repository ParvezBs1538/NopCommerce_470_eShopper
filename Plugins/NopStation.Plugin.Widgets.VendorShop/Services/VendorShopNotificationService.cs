using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public class VendorShopNotificationService : IVendorShopNotificationService
    {
        private readonly IVendorSubscriberService _vendorSubscriberService;
        private readonly ICustomerService _customerService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILocalizationService _localizationService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILanguageService _languageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        public VendorShopNotificationService(IVendorSubscriberService vendorSubscriberService,
            ICustomerService customerService,
            IMessageTokenProvider messageTokenProvider,
            IWorkflowMessageService workflowMessageService,
            ILocalizationService localizationService,
            IEmailAccountService emailAccountService,
            ILanguageService languageService,
            LocalizationSettings localizationSettings,
            EmailAccountSettings emailAccountSettings,
            IStoreService storeService,
            IWorkContext workContext)
        {
            _vendorSubscriberService = vendorSubscriberService;
            _customerService = customerService;
            _messageTokenProvider = messageTokenProvider;
            _workflowMessageService = workflowMessageService;
            _localizationService = localizationService;
            _emailAccountService = emailAccountService;
            _languageService = languageService;
            _localizationSettings = localizationSettings;
            _emailAccountSettings = emailAccountSettings;
            _storeService = storeService;
            _workContext = workContext;
        }
        public MessageTemplate PrepareMessageTemplate(string name, string subject, string body, int delayHour)
        {
            var messageTemplate = new MessageTemplate
            {
                Name = name,
                Subject = subject,
                Body = body,
                DelayPeriod = MessageDelayPeriod.Hours,
                DelayBeforeSend = delayHour
            };

            return messageTemplate;
        }

        protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                               (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            return emailAccount;
        }

        protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
        {
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language == null || !language.Published)
            {
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");

            return language.Id;
        }


        private async Task<IList<Token>> PrepareTokenAsync(int subscribedId, IList<Token> commonTokens)
        {
            var subscriber = await _vendorSubscriberService.GetVendorSubscriberByIdAsync(subscribedId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, subscriber.CustomerId);
            return commonTokens;
        }

        public async Task SendEmailAsync(IList<int> subscribedIds, MessageTemplate messageTemplate, int storeId)
        {
            var languageId = await EnsureLanguageIsActiveAsync(_localizationSettings.DefaultAdminLanguageId, storeId);
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
            var storeTokens = new List<Token>();
            var store = await _storeService.GetStoreByIdAsync(storeId);
            if (store != null)
            {
                await _messageTokenProvider.AddStoreTokensAsync(storeTokens, store, emailAccount);
            }
            var vendor = await _workContext.GetCurrentVendorAsync();
            if (vendor != null)
            {
                await _messageTokenProvider.AddVendorTokensAsync(storeTokens, vendor);
            }

            var commonToken = storeTokens;
            foreach (var id in subscribedIds)
            {
                var tokens = await PrepareTokenAsync(id, commonToken);
                var subscriber = await _vendorSubscriberService.GetVendorSubscriberByIdAsync(id);
                if (subscriber == null)
                    continue;
                if (store == null)
                {
                    store = await _storeService.GetStoreByIdAsync(subscriber.StoreId);
                    await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                }
                var customer = await _customerService.GetCustomerByIdAsync(subscriber.CustomerId);
                var toName = customer.FirstName + " " + customer.LastName;
                var toEmail = customer.Email;
                await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }
        }
    }
}
