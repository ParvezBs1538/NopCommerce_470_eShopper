using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Services.Messages;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services.Messages
{
    public class AbandonedCartMessageService : IAbandonedCartMessageService
    {
        #region Fields

        private readonly ITokenizer _tokenizer;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IStoreContext _storeContext;
        private readonly IAbandonedCartMessageTokenProvider _abandonedCartMessageTokenProvider;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public AbandonedCartMessageService(ITokenizer tokenizer,
            IQueuedEmailService queuedEmailService,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            IMessageTemplateService messageTemplateService,
            IAbandonedCartMessageTokenProvider abandonedCartMessageTokenProvider,
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            ICustomerService customerService)
        {
            _tokenizer = tokenizer;
            _queuedEmailService = queuedEmailService;
            _storeContext = storeContext;
            _eventPublisher = eventPublisher;
            _messageTemplateService = messageTemplateService;
            _abandonedCartMessageTokenProvider = abandonedCartMessageTokenProvider;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        public async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
        {
            //get message templates by the name
            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

            //no template found
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            //filter active templates
            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

            return messageTemplates;
        }

        #endregion

        #region Methods

        public async Task SendCustomerEmailAsync(Customer customer, IList<ProductInfoModel> productInfoModels, string jwtToken, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //store
            var store = await _storeContext.GetCurrentStoreAsync();

            var messageTemplates = await GetActiveMessageTemplatesAsync(AbandonedCartMessageTemplateSystemNames.ABANDONED_CARTS_CUSTOMER_NOTIFICATION, store.Id);
            if (!messageTemplates.Any())
                throw new ArgumentNullException(nameof(messageTemplates));

            //only single messageTemplate
            var messageTemplate = messageTemplates[messageTemplates.Count() - 1];

            //email account
            var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(messageTemplate.EmailAccountId) ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                                (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

            //tokens
            IList<Token> tokens = new List<Token>();
            await _abandonedCartMessageTokenProvider.AddCustomerTokensAsync(tokens, customer.Id, jwtToken);
            await _abandonedCartMessageTokenProvider.AddProductTokensAsync(tokens, productInfoModels, languageId);
            await _abandonedCartMessageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var body = messageTemplate.Body;
            var subject = messageTemplate.Subject;

            //Replace subject and body tokens
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);
            // custom code
            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = customer.Email,
                ToName = await _customerService.GetCustomerFullNameAsync(customer),
                ReplyTo = null,
                ReplyToName = null,
                CC = string.Empty,
                Bcc = null,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = 1,
                DontSendBeforeDateUtc = null
            };

            await _queuedEmailService.InsertQueuedEmailAsync(email);
        }

        #endregion
    }
}