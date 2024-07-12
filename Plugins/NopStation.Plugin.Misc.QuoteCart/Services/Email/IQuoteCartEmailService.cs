using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services.Email;

public interface IQuoteCartEmailService
{
    Task<IList<int>> SendEmailAsync(QuoteRequest request, QuoteRequestNotificationType quoteRequestNotificationType, string recipientName, string recipientEmail, string attachmentName = null, string attachmentPath = null, string message = null);

    Task<IList<int>> SendEmailToStoreOwner(QuoteRequest quoteRequest, QuoteRequestNotificationType quoteRequestNotificationType, string message = null, string attachmentName = null, string attachmentPath = null);
}
