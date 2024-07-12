using System.Threading.Tasks;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Factories;

public interface IPublicQuoteRequestModelFactory
{
    Task<QuoteRequestDetailsModel> PrepareRequestDetailsModelAsync(QuoteRequestDetailsModel model, QuoteRequest request, bool loadMessages = false);

    Task<QuoteRequestListModel> PrepareRequestListModelAsync(int? page);

    //Task<QuoteFormModel> PrepareQuoteFormModelByIdAsync(QuoteForm quoteForm, string attributesXml = "");
}