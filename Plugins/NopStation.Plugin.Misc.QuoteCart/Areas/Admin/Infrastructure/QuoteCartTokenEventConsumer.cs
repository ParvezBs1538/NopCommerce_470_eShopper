using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Messages;
using Nop.Services.Events;
using Nop.Services.Messages;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Infrastructure;

public class QuoteCartTokenEventConsumer : IConsumer<AdditionalTokensAddedEvent>
{
    #region Fields

    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public static readonly List<string> CommonTokens = [
        "%Store.Name%",
        "%Store.URL%",
        "%Store.Email%",
        "%Store.CompanyName%",
        "%Store.CompanyAddress%",
        "%Store.CompanyPhoneNumber%",
        "%Store.CompanyVat%",
        "%Facebook.URL%",
        "%Twitter.URL%",
        "%YouTube.URL%",
        "%Customer.Email%",
        "%Customer.Username%",
        "%Customer.FullName%",
        "%Customer.FirstName%",
        "%Customer.LastName%",
        "%Customer.VatNumber%",
        "%Customer.VatNumberStatus%",
        "%Customer.CustomAttributes%",
        "%Customer.PasswordRecoveryURL%",
        "%Customer.AccountActivationURL%",
        "%Customer.EmailRevalidationURL%",
        "%Wishlist.URLForCustomer%",
        "%QuoteRequest.Form%",
        "%QuoteRequest.AdminUrl%",
    ];

    #endregion

    #region Ctor

    public QuoteCartTokenEventConsumer(
        IHttpContextAccessor httpContextAccessor,
        IMessageTemplateService messageTemplateService)
    {
        _httpContextAccessor = httpContextAccessor;
        _messageTemplateService = messageTemplateService;
    }

    #endregion

    #region Methods

    public async Task HandleEventAsync(AdditionalTokensAddedEvent eventMessage)
    {
        if (!int.TryParse(_httpContextAccessor.HttpContext.GetRouteValue("id")?.ToString(), out var templateId))
            return;

        var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(templateId);
        switch (messageTemplate?.Name)
        {
            case QuoteCartDefaults.REQUEST_NEW_REPLY_CUSTOMER_NOTIFICATION:
            case QuoteCartDefaults.REQUEST_NEW_REPLY_STORE_NOTIFICATION:
            case QuoteCartDefaults.QUOTE_CUSTOMER_QUOTE_OFFER:
            case QuoteCartDefaults.QUOTE_CUSTOMER_REQUEST_SUBMITTED_NOTIFICATION:
            case QuoteCartDefaults.QUOTE_STORE_REQUEST_SUBMITTED_NOTIFICATION:
            case QuoteCartDefaults.REQUEST_CUSTOMER_STATUS_CHANGED_NOTIFICATION:
                CommonTokens.ForEach(eventMessage.AdditionalTokens.Add);
                eventMessage.AddTokens(
                    "%QuoteRequest.RequestProductsTable%",
                    "%QuoteRequest.ID%",
                    "%QuoteRequest.Url%",
                    "%QuoteRequest.Status%");
                if (messageTemplate.Name == QuoteCartDefaults.REQUEST_NEW_REPLY_CUSTOMER_NOTIFICATION || messageTemplate.Name == QuoteCartDefaults.REQUEST_NEW_REPLY_STORE_NOTIFICATION)
                    eventMessage.AddTokens("%QuoteRequest.Message%");
                break;
            default:
                break;
        }
    }

    #endregion
}
