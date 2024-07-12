using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;

namespace NopStation.Plugin.Misc.QuoteCart.Services.Email;

public class QuoteCartEmailService : IQuoteCartEmailService
{
    #region Fields 

    private readonly IQuoteFormService _quoteFormService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly IStoreService _storeService;
    private readonly IQuoteRequestService _quoteRequestService;
    private readonly IStoreContext _storeContext;
    private readonly ILanguageService _languageService;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly ILocalizationService _localizationService;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IFormAttributeFormatter _formAttributeFormatter;
    private readonly IProductService _productService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IPictureService _pictureService;
    private readonly ICustomerService _customerService;
    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly TaxSettings _taxSettings;
    private readonly IMessageTokenProvider _messageTokenProvider;


    #endregion

    #region Ctor 

    public QuoteCartEmailService(
        IQuoteFormService quoteCartFormService,
        IEmailAccountService emailAccountService,
        IWorkflowMessageService workflowMessageService,
        IStoreService storeService,
        IQuoteRequestService quoteRequestService,
        IStoreContext storeContext,
        ILanguageService languageService,
        IMessageTemplateService messageTemplateService,
        ILocalizationService localizationService,
        IActionContextAccessor actionContextAccessor,
        IEventPublisher eventPublisher,
        IUrlHelperFactory urlHelperFactory,
        IFormAttributeFormatter formAttributeFormatter,
        IProductService productService,
        IPriceFormatter priceFormatter,
        IPictureService pictureService,
        ICustomerService customerService,
        EmailAccountSettings emailAccountSettings,
        TaxSettings taxSettings,
        IMessageTokenProvider messageTokenProvider)
    {
        _quoteFormService = quoteCartFormService;
        _emailAccountService = emailAccountService;
        _workflowMessageService = workflowMessageService;
        _storeService = storeService;
        _quoteRequestService = quoteRequestService;
        _storeContext = storeContext;
        _languageService = languageService;
        _messageTemplateService = messageTemplateService;
        _localizationService = localizationService;
        _actionContextAccessor = actionContextAccessor;
        _eventPublisher = eventPublisher;
        _urlHelperFactory = urlHelperFactory;
        _formAttributeFormatter = formAttributeFormatter;
        _productService = productService;
        _priceFormatter = priceFormatter;
        _pictureService = pictureService;
        _customerService = customerService;
        _emailAccountSettings = emailAccountSettings;
        _taxSettings = taxSettings;
        _messageTokenProvider = messageTokenProvider;
    }

    #endregion

    #region Methods

    public async Task<IList<int>> SendEmailAsync(
        QuoteRequest request,
        QuoteRequestNotificationType quoteRequestNotificationType,
        string recipientName,
        string recipientEmail,
        string attachmentName = null,
        string attachmentPath = null,
        string message = null)
    {
        if (string.IsNullOrEmpty(recipientEmail))
            return [];

        var form = await _quoteFormService.GetFormByIdAsync(request.FormId);

        if (form == null)
            return [];

        var store = await _storeService.GetStoreByIdAsync(request.StoreId) ?? await _storeContext.GetCurrentStoreAsync();

        var languageId = store.DefaultLanguageId;

        var emailAcc = await _emailAccountService.GetEmailAccountByIdAsync(form.DefaultEmailAccountId);

        var templateName = quoteRequestNotificationType switch
        {
            QuoteRequestNotificationType.CustomerRequestSubmitted => QuoteCartDefaults.QUOTE_CUSTOMER_REQUEST_SUBMITTED_NOTIFICATION,
            QuoteRequestNotificationType.StoreRequestSubmitted => QuoteCartDefaults.QUOTE_STORE_REQUEST_SUBMITTED_NOTIFICATION,
            QuoteRequestNotificationType.CustomerReplySent => QuoteCartDefaults.REQUEST_NEW_REPLY_CUSTOMER_NOTIFICATION,
            QuoteRequestNotificationType.StoreReplySent => QuoteCartDefaults.REQUEST_NEW_REPLY_STORE_NOTIFICATION,
            QuoteRequestNotificationType.CustomerStatusChanged => QuoteCartDefaults.REQUEST_CUSTOMER_STATUS_CHANGED_NOTIFICATION,
            QuoteRequestNotificationType.QuoteOffer => QuoteCartDefaults.QUOTE_CUSTOMER_QUOTE_OFFER,
            _ => throw new NopException("Unsupported email type.")
        };

        var messageTemplates = await GetActiveMessageTemplatesAsync(templateName, store.Id);

        //tokens
        var commonTokens = new List<Token>();
        var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);
        await AddRequestTokensAsync(commonTokens, request, customer, message);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, request.CustomerId);
        await AddQuoteFormAttributeTokensAsync(commonTokens, form, request.AttributeXml);

        if (await _customerService.IsGuestAsync(customer))
        {
            await AddGuestTokensAsync(commonTokens, request);
        }

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccountId = form.DefaultEmailAccountId;
            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ??
                await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId) ??
                throw new NopException("No email configured for form");

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);


            string attachmentFileName = null;
            string attachmentFilePath = null;

            return await _workflowMessageService.SendNotificationAsync(
                messageTemplate,
                emailAccount,
                languageId,
                tokens,
                recipientEmail,
                recipientName,
                attachmentFilePath: attachmentFilePath,
                attachmentFileName: attachmentFileName);
        }).ToListAsync();
    }

    public async Task<IList<int>> SendEmailToStoreOwner(
        QuoteRequest quoteRequest,
        QuoteRequestNotificationType quoteRequestNotificationType,
        string message = null,
        string attachmentName = null,
        string attachmentPath = null)
    {
        switch (quoteRequestNotificationType)
        {
            case QuoteRequestNotificationType.CustomerReplySent:
            case QuoteRequestNotificationType.CustomerStatusChanged:
            case QuoteRequestNotificationType.CustomerRequestSubmitted:
                return [];
            default:
                break;
        }

        var form = await _quoteFormService.GetFormByIdAsync(quoteRequest.FormId);

        if (form == null)
            return [];

        var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(form.DefaultEmailAccountId) ??
            await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId) ??
            throw new NopException("No email configured for form");

        return await SendEmailAsync(quoteRequest, quoteRequestNotificationType, emailAccount.DisplayName, emailAccount.Email, attachmentName, attachmentPath, message);
    }

    #endregion

    #region Utilities 

    protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
    {
        //load language by specified ID
        var language = await _languageService.GetLanguageByIdAsync(languageId);

        if (language == null || !language.Published)
            //load any language from the specified store
            language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();

        if (language == null || !language.Published)
            //load any language
            language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();

        if (language == null)
            throw new Exception("No active language could be loaded");

        return language.Id;
    }

    protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
    {
        //get message templates by the name
        var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

        //no template found
        if (messageTemplates?.Count is null or 0)
            return [];

        //filter active templates
        messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

        return messageTemplates;
    }

    protected virtual async Task AddGuestTokensAsync(IList<Token> tokens, QuoteRequest quoteRequest)
    {
        tokens.Add(new Token("Customer.Email", quoteRequest.GuestEmail));
        tokens.Add(new Token("Customer.Username", quoteRequest.GuestEmail));
        tokens.Add(new Token("Customer.FullName", await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.QuoteRequests.WelcomeCustomer")));
    }

    protected virtual async Task AddRequestTokensAsync(IList<Token> tokens, QuoteRequest request, Customer customer, string message = null)
    {
        var store = await _storeService.GetStoreByIdAsync(request.StoreId) ?? await _storeContext.GetCurrentStoreAsync();

        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        var url = store.Url.TrimEnd('/') + urlHelper.RouteUrl("QuoteCart.RequestDetails", new { requestId = request.RequestId });
        var adminUrl = store.Url.TrimEnd('/') + "/Admin/QCQuoteRequest/Edit/" + request.Id.ToString();

        tokens.Add(new Token("QuoteRequest.ID", request.Id));
        tokens.Add(new Token("QuoteRequest.Url", url));
        tokens.Add(new Token("QuoteRequest.Status", request.RequestStatus));
        tokens.Add(new Token("QuoteRequest.AdminUrl", adminUrl));

        if (!string.IsNullOrEmpty(message))
            tokens.Add(new Token("QuoteRequest.Message", message));

        tokens.Add(new Token("QuoteRequest.RequestProductsTable", await ParseProductInfoForTokenAsync(tokens, request, customer), true));
    }

    protected virtual async Task<string> ParseProductInfoForTokenAsync(IList<Token> tokens, QuoteRequest request, Customer customer)
    {
        var requestItems = await _quoteRequestService.GetItemsByQuoteRequestId(request.Id);

        async Task<string> getTable(string tableRows) =>
            $@"<table border=""0"" style=""width:100%;"">
                        <thead>
                            <tr style=""text-align:center;background-color:#B9BABE;"">
                                <th>{await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.Cart.SKU")}</th>
                                <th>{await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.Cart.Image")}</th>
                                <th>{await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.Cart.Product(s)")}</th>
                                <th>{await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.Cart.UnitPrice")}</th>
                                <th>{await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.Cart.Quantity")}</th>
                                <th>{await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.Cart.ItemTotal")}</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tableRows}
                        </tbody>
                        <tfoot>
                            <tr>
                                <td colspan=""5"" style=""text-align:right;background-color:#DDE2E6;padding:0.6em 0.4em;""><strong>{await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.Cart.Total")}:</strong></td>
                                <td style=""text-align:center;background-color:#DDE2E6;padding:0.6em 0.4em;""><strong>%QuoteOffer.TotalPrice%</strong></td>
                            </tr>
                        </tfoot>
                    </table>";

        var productsRows = "";

        var tableRowTemplate =
            $@"<tr style=""text-align:center;background-color:#EBECEE;"">
                        <td style=""text-align:center;padding:0.6em 0.4em;"">%Quote.Product.SKU%</td>
                        <td style=""text-align:center;padding:0.6em 0.4em;""><img src=""%Quote.Product.Picture%"" alt=""%Quote.Product.Name%"" width=""100"" height=""100""/></td>
                        <td style=""text-align:left;padding:0.6em 0.4em;"">%Quote.Product.Name%</td>
                        <td style=""text-align:center;padding:0.6em 0.4em;"">%Quote.Product.UnitPrice%</td>
                        <td style=""text-align:center;padding:0.6em 0.4em;"">%Quote.Product.Quantity%</td>
                        <td style=""text-align:center;padding:0.6em 0.4em;"">%Quote.Product.TotalPrice%</td>
                    </tr>";

        foreach (var item in requestItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);

            var productPictureUrl = "";

            if (pictures.Any())
            {
                productPictureUrl = await _pictureService.GetPictureUrlAsync(pictures.First().Id);
            }
            else
            {
                productPictureUrl = await _pictureService.GetDefaultPictureUrlAsync();
            }


            if (product != null)
            {
                var (priceWithTax, priceWithoutTax) = await _quoteRequestService.GetRequestItemPriceAsync(item, customer);
                productsRows += tableRowTemplate
                    .Replace("%Quote.Product.Picture%", productPictureUrl)
                    .Replace("%Quote.Product.Name%", product.Name)
                    .Replace("%Quote.Product.SKU%", product.Sku)
                    .Replace("%Quote.Product.UnitPrice%", await _priceFormatter.FormatPriceAsync(_taxSettings.PricesIncludeTax ? priceWithTax : priceWithoutTax))
                    .Replace("%Quote.Product.Quantity%", item.Quantity.ToString())
                    .Replace("%Quote.Product.TotalPrice%", await _priceFormatter.FormatPriceAsync(_taxSettings.PricesIncludeTax ? priceWithTax * item.Quantity : priceWithoutTax * item.Quantity));
            }
        }

        var (subTotalWithTax, subTotalWithoutTax) = await _quoteRequestService.GetRequestSubTotalAsync(request);
        var totalPrice = _taxSettings.ForceTaxExclusionFromOrderSubtotal ? subTotalWithoutTax : subTotalWithTax;
        tokens.Add(new Token("QuoteOffer.TotalPrice", await _priceFormatter.FormatPriceAsync(totalPrice)));

        var productTable = await getTable(productsRows);

        productTable = productTable.Replace("%QuoteOffer.TotalPrice%", await _priceFormatter.FormatPriceAsync(totalPrice));

        return productTable;
    }

    protected virtual async Task AddQuoteFormAttributeTokensAsync(IList<Token> tokens, QuoteForm quoteForm, string attributeXml)
    {
        ArgumentNullException.ThrowIfNull(quoteForm);

        var attributes = await _formAttributeFormatter.FormatAttributesAsync(quoteForm, attributeXml);

        tokens.Add(new Token("QuoteRequest.FormAttributes", attributes, true));
    }

    #endregion
}
