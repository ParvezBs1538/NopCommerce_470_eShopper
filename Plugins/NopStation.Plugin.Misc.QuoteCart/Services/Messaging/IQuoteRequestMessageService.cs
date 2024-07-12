using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services.RequestMessage;

public interface IQuoteRequestMessageService
{
    Task<IPagedList<QuoteRequestMessage>> GetAllQuoteRequestMessagesAsync(int requestId = 0, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<QuoteRequestMessage> GetQuoteRequestMessageByIdAsync(int messageId);

    Task InsertQuoteRequestMessageAsync(QuoteRequestMessage requestMessage);

    Task UpdateQuoteRequestMessageAsync(QuoteRequestMessage requestMessage);

    Task DeleteQuoteRequestMessageAsync(QuoteRequestMessage requestMessage);
}
