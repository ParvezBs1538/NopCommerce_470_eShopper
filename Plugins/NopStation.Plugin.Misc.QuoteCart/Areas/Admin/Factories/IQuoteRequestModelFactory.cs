using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Messages;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;

public interface IQuoteRequestModelFactory
{
    Task<QuoteRequestSearchModel> PrepareQuoteRequestSearchModelAsync(QuoteRequestSearchModel searchModel);

    Task<QuoteRequestListModel> PrepareQuoteRequestListModelAsync(QuoteRequestSearchModel searchModel);

    Task<QuoteRequestDetailsModel> PrepareQuoteRequestModelAsync(QuoteRequestDetailsModel customerRequestDetailsModel, QuoteRequest quoteRequest, bool excludeProperties = false);

    Task<MessageTemplateListModel> PrepareMessageTemplateListModelAsync(MessageTemplateSearchModel searchModel);

    Task<QuoteRequestItemModel> PrepareQuoteRequestItemModelAsync(QuoteRequestItemModel model, QuoteRequestItem quoteRequestItem, QuoteRequest quoteRequest, bool excludeProperties = true);

    Task<ConvertToOrderModel> PrepareConvertToOrderModelAsync(ConvertToOrderModel model, QuoteRequest quoteRequest);

    Task<IList<SubmittedFormAttributeModel>> PrepareSubmittedFormAttributeModelsAsync(QuoteRequest quoteRequest);

    Task PrepareShippingMethodsAsync(IList<SelectListItem> items, QuoteRequest quoteRequest, string providerSystemName = "", int shippingAddressId = 0, bool addDefaultItem = false);
}