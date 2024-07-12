using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Factories;

public interface IQuoteCartModelFactory
{
    Task<PictureModel> PrepareCartItemPictureModelAsync(QuoteCartItem qci, int pictureSize, bool showDefaultPicture, string productName);

    public Task<CartModel> PrepareQuoteCartModelAsync(CartModel model,
       IList<QuoteCartItem> cart, IList<QuoteForm> quoteForms);

    Task<QuoteFormModel> PrepareQuoteFormModelAsync(QuoteFormModel model, QuoteForm quoteForm, string attributesXml = "");
}
