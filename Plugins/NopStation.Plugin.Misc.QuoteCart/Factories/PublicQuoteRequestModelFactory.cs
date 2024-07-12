using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Models.Common;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Models;
using NopStation.Plugin.Misc.QuoteCart.Services;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;
using NopStation.Plugin.Misc.QuoteCart.Services.RequestMessage;

namespace NopStation.Plugin.Misc.QuoteCart.Factories;

public partial class PublicQuoteRequestModelFactory : IPublicQuoteRequestModelFactory
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IQuoteRequestService _quoteRequestService;
    private readonly IHtmlFormatter _htmlFormatter;
    private readonly IWorkContext _workContext;
    private readonly IQuoteRequestMessageService _quoteRequestMessageService;
    private readonly IPictureService _pictureService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IProductService _productService;
    private readonly ILocalizationService _localizationService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IStoreService _storeService;
    private readonly IFormAttributeParser _formAttributeParser;
    private readonly IFormAttributeService _formAttributeService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public PublicQuoteRequestModelFactory(
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IQuoteRequestService quoteRequestService,
        IHtmlFormatter htmlFormatter,
        IWorkContext workContext,
        IQuoteRequestMessageService quoteRequestMessageService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        ILocalizationService localizationService,
        IUrlRecordService urlRecordService,
        IStoreService storeService,
        IFormAttributeParser formAttributeParser,
        IFormAttributeService formAttributeService,
        IPermissionService permissionService)
    {
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _quoteRequestService = quoteRequestService;
        _htmlFormatter = htmlFormatter;
        _workContext = workContext;
        _quoteRequestMessageService = quoteRequestMessageService;
        _pictureService = pictureService;
        _priceFormatter = priceFormatter;
        _productService = productService;
        _localizationService = localizationService;
        _urlRecordService = urlRecordService;
        _storeService = storeService;
        _formAttributeParser = formAttributeParser;
        _formAttributeService = formAttributeService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual async Task<QuoteRequestListModel> PrepareRequestListModelAsync(int? pageNumber)
    {
        var pageIndex = pageNumber > 0 ? pageNumber.Value - 1 : 0;
        var customer = await _workContext.GetCurrentCustomerAsync();

        var requests = await _quoteRequestService.GetAllQuoteRequestsByCustomerIdAsync(
            customerId: customer.Id,
            pageIndex: pageIndex,
            pageSize: QuoteCartDefaults.ACCOUNT_REQUESTS_PAGE_SIZE);

        var customerRequestListModel = new QuoteRequestListModel();
        foreach (var rq in requests)
        {
            var model = new QuoteRequestModel
            {
                Id = rq.Id,
                QuoteRequestStatusId = rq.RequestStatusId,
                RequestId = rq.RequestId,
                CreatedOn = rq.CreatedOnUtc,
                QuoteRequestStatus = await _localizationService.GetLocalizedEnumAsync(rq.RequestStatus),
            };

            var requestItems = await _quoteRequestService.GetItemsByQuoteRequestId(rq.Id);

            if (requestItems.Count == 0)
                model.ProductsOverview = string.Empty;
            else
            {
                var products = await _productService.GetProductsByIdsAsync(requestItems.Select(x => x.ProductId).Take(3).ToArray());

                if (requestItems.Count > 3)
                    model.ProductsOverview = string.Format(await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.ProductsOverview"), string.Join(", ", products.Select(x => x.Name)), requestItems.Count - 3);
                else
                    model.ProductsOverview = string.Join(", ", products.Select(x => x.Name));
            }

            customerRequestListModel.QuoteRequests.Add(model);
        }

        customerRequestListModel.PagerModel = new PagerModel(_localizationService)
        {
            PageSize = requests.PageSize,
            TotalRecords = requests.TotalCount,
            PageIndex = requests.PageIndex,
            ShowTotalSummary = false,
            RouteActionName = "AllRequest",
            RouteValues = new QuoteRequestListModel.QuoteRequestRouteValues()
        };

        return customerRequestListModel;
    }

    #region Submitted form attribute

    public async Task<IList<SubmittedFormAttributeModel>> PrepareSubmittedFormAttributeModelsAsync(QuoteRequest quoteRequest)
    {
        var model = new List<SubmittedFormAttributeModel>();

        var attributes = await _formAttributeParser.ParseFormAttributeMappingsAsync(quoteRequest.AttributeXml);
        foreach (var attribute in attributes)
        {
            var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(attribute.FormAttributeId);
            var values = _formAttributeParser.ParseValues(quoteRequest.AttributeXml, attribute.Id);
            var attributeModel = new SubmittedFormAttributeModel
            {
                Name = await _localizationService.GetLocalizedAsync(formAttribute, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(formAttribute, x => x.Description),
                AttributeControlType = attribute.AttributeControlType,
            };

            if (attribute.AttributeControlType == AttributeControlType.DropdownList ||
                attribute.AttributeControlType == AttributeControlType.RadioList ||
                attribute.AttributeControlType == AttributeControlType.Checkboxes ||
                attribute.AttributeControlType == AttributeControlType.ReadonlyCheckboxes ||
                attribute.AttributeControlType == AttributeControlType.ColorSquares ||
                attribute.AttributeControlType == AttributeControlType.ImageSquares)
            {
                foreach (var value in values)
                {
                    var formAttributeValue = await _formAttributeService.GetFormAttributeValueByIdAsync(int.Parse(value));
                    if (formAttributeValue != null)
                    {
                        if (attribute.AttributeControlType == AttributeControlType.ColorSquares)
                            attributeModel.Values.Add(formAttributeValue.ColorSquaresRgb);
                        else if (attribute.AttributeControlType == AttributeControlType.ImageSquares)
                            attributeModel.Values.Add(await _pictureService.GetPictureUrlAsync(formAttributeValue.ImageSquaresPictureId, 100));
                        else
                            attributeModel.Values.Add(await _localizationService.GetLocalizedAsync(formAttributeValue, x => x.Name));
                    }
                }
            }
            else
            {
                attributeModel.Values = values;
            }

            model.Add(attributeModel);
        }

        return model;
    }

    #endregion

    public virtual async Task<QuoteRequestDetailsModel> PrepareRequestDetailsModelAsync(
        QuoteRequestDetailsModel model,
        QuoteRequest request,
        bool loadMessages = false)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (request == null)
            return model;

        model = new QuoteRequestDetailsModel()
        {
            Id = request.Id,
            RequestStatusId = request.RequestStatusId,
            RequestId = request.RequestId,
            CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(request.CreatedOnUtc, DateTimeKind.Utc),
            UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(request.UpdatedOnUtc, DateTimeKind.Utc),
            CustomerId = request.CustomerId,
            RequestStatus = await _localizationService.GetLocalizedEnumAsync(request.RequestStatus),
            GuestEmail = request.GuestEmail,
        };

        var store = await _storeService.GetStoreByIdAsync(request.StoreId);
        model.Store = store.Name;

        var quoteRequestItems = await _quoteRequestService.GetItemsByQuoteRequestId(request.Id);

        model.QuoteRequestItems = await quoteRequestItems.SelectAwait(async x => await PrepareQuoteRequestItemModelAsync(new(), x)).ToListAsync();

        model.RequestType = "Request for Quotation";

        var (subTotalWithTax, subTotalWithoutTax) = await _quoteRequestService.GetRequestSubTotalAsync(request);
        model.RequestTotals = await _priceFormatter.FormatPriceAsync(subTotalWithoutTax);

        if (loadMessages)
        {
            model.RequestMessages = await PrepareQuoteRequestMessageModelsAsync(request);
        }


        model.SubmittedFormAttributes = await PrepareSubmittedFormAttributeModelsAsync(request);
        if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            model.CustomProperties.Add("DisplayPrice", "True");
        else
            model.CustomProperties.Add("DisplayPrice", "False");

        return model;
    }


    public async Task<QuoteRequestItemModel> PrepareQuoteRequestItemModelAsync(QuoteRequestItemModel model, QuoteRequestItem quoteRequestItem)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (quoteRequestItem != null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            model = quoteRequestItem.ToModel<QuoteRequestItemModel>();
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            model.ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            model.ProductSeName = await _urlRecordService.GetSeNameAsync(product);
            model.ProductSku = product.Sku;
            model.ProductPrice = await _priceFormatter.FormatPriceAsync(product.Price);
            model.ItemTotalValue = product.Price * quoteRequestItem.Quantity;
            model.ItemTotal = await _priceFormatter.FormatPriceAsync(model.ItemTotalValue);
            model.DiscountedPriceValue = quoteRequestItem.DiscountedPrice;
            model.DiscountedPrice = await _priceFormatter.FormatPriceAsync(quoteRequestItem.DiscountedPrice);
            if (product.IsRental)
            {
                model.RentalInfo =
                string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                    quoteRequestItem.RentalStartDateUtc, quoteRequestItem.RentalEndDateUtc);
            }
        }

        return model;
    }

    public async Task<IList<QuoteRequestMessageModel>> PrepareQuoteRequestMessageModelsAsync(QuoteRequest quoteRequest)
    {
        var messages = await _quoteRequestMessageService.GetAllQuoteRequestMessagesAsync(quoteRequest.Id);

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        return await messages.SelectAwait(async x =>
        {
            var model = x.ToModel<QuoteRequestMessageModel>();
            model.Content = _htmlFormatter.FormatText(model.Content, false, true, true, true, false, false);
            model.IsWriter = currentCustomer.Id == x.CustomerId;
            var customer = await _customerService.GetCustomerByIdAsync(x.CustomerId);
            model.CustomerEmail = string.IsNullOrEmpty(customer.Email) ? quoteRequest.GuestEmail : customer.Email;
            model.CustomerName = customer != null ? await _customerService.GetCustomerFullNameAsync(customer) : await _localizationService.GetResourceAsync("Customer.Guest");
            model.SentOn = await _dateTimeHelper.ConvertToUserTimeAsync(x.CreatedOnUtc, DateTimeKind.Utc);
            return model;
        }).ToListAsync();
    }

    #endregion
}
