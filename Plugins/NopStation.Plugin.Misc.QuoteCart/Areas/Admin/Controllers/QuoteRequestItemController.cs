using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Controllers;

public class QuoteRequestItemController : NopStationAdminController
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductService _productService;
    private readonly IQuoteRequestItemService _quoteRequestItemService;
    private readonly IQuoteRequestModelFactory _quoteRequestModelFactory;
    private readonly IQuoteRequestService _quoteRequestService;
    private readonly IShoppingCartService _shoppingCartService;

    #endregion

    #region Ctor

    public QuoteRequestItemController(
        ICustomerService customerService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IQuoteRequestItemService quoteRequestItemService,
        IQuoteRequestModelFactory quoteRequestModelFactory,
        IQuoteRequestService quoteRequestService,
        IShoppingCartService shoppingCartService)
    {
        _customerService = customerService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _productAttributeFormatter = productAttributeFormatter;
        _productAttributeParser = productAttributeParser;
        _productModelFactory = productModelFactory;
        _productService = productService;
        _quoteRequestItemService = quoteRequestItemService;
        _quoteRequestModelFactory = quoteRequestModelFactory;
        _quoteRequestService = quoteRequestService;
        _shoppingCartService = shoppingCartService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Create(int requestId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var quoteRequest = await _quoteRequestService.GetQuoteRequestByIdAsync(requestId);

        if (quoteRequest == null)
            return RedirectToAction("List", "QCQuoteRequest");

        var model = await _quoteRequestModelFactory.PrepareQuoteRequestItemModelAsync(new QuoteRequestItemModel()
        {
            ProductId = productId
        }, null, quoteRequest, productId <= 0);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(QuoteRequestItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var quoteRequest = await _quoteRequestService.GetQuoteRequestByIdAsync(model.QuoteRequestId) ?? throw new ArgumentException("No quote request found with the specified id");

        if (ModelState.IsValid)
        {

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(model.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId) ?? throw new ArgumentException("No customer found with the specified id");

            var quantity = Math.Max(model.Quantity, product.OrderMinimumQuantity);

            if (quantity <= 0)
                quantity = 1;

            //warnings
            var warnings = new List<string>();

            //attributes
            var attributesXml = await _productAttributeParser.ParseProductAttributesAsync(product, Request.Form, warnings);

            //warnings
            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(customer, ShoppingCartType.ShoppingCart, product, quantity, attributesXml));
            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemGiftCardWarningsAsync(ShoppingCartType.ShoppingCart, product, attributesXml));

            if (warnings.Count == 0)
            {
                //save item
                var quoteRequestItem = new QuoteRequestItem()
                {
                    QuoteRequestId = quoteRequest.Id,
                    ProductId = product.Id,
                    Quantity = quantity,
                    AttributesXml = attributesXml,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    CustomerId = customer.Id,
                    DiscountedPrice = model.DiscountedPrice > 0 ? model.DiscountedPrice : product.Price,
                    StoreId = quoteRequest.StoreId
                };

                await _quoteRequestItemService.InsertQuoteRequestItemAsync(quoteRequestItem);

                return RedirectToAction("Edit", "QCQuoteRequest", new { id = quoteRequest.Id });
            }

            foreach (var warning in warnings)
            {
                _notificationService.WarningNotification(warning);
            }
        }

        model = await _quoteRequestModelFactory.PrepareQuoteRequestItemModelAsync(model, null, quoteRequest, model.ProductId <= 0);

        return View(model);
    }

    public virtual async Task<IActionResult> ProductSelectPopup()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductSelectPopup(int selectedProductId)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var product = await _productService.GetProductByIdAsync(selectedProductId);
        if (product != null)
        {
            ViewBag.RefreshPage = true;
            ViewBag.ProductId = selectedProductId;
        }
        //prepare model
        var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());

        return View(model);
    }

    public async Task<IActionResult> Edit(int id, int requestId)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var requestItem = await _quoteRequestItemService.GetQuoteRequestItemByIdAsync(id);
        var quoteRequest = await _quoteRequestService.GetQuoteRequestByIdAsync(requestId);

        if (requestItem == null || quoteRequest == null)
            return RedirectToAction("Edit", "QCQuoteRequest", new { id = requestId });

        var model = await _quoteRequestModelFactory.PrepareQuoteRequestItemModelAsync(null, requestItem, quoteRequest);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(QuoteRequestItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var requestItem = await _quoteRequestItemService.GetQuoteRequestItemByIdAsync(model.Id);

        if (requestItem == null)
            return RedirectToAction("Edit", "QCQuoteRequest", new { id = model.QuoteRequestId });

        requestItem.Quantity = Math.Abs(model.Quantity);
        requestItem.DiscountedPrice = model.DiscountedPrice;
        requestItem.UpdatedOnUtc = DateTime.UtcNow;
        await _quoteRequestItemService.UpdateQuoteRequestItemAsync(requestItem);
        return RedirectToAction("Edit", "QCQuoteRequest", new { id = model.QuoteRequestId });

    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("delete")]
    public async Task<IActionResult> Delete(QuoteRequestItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var requestItem = await _quoteRequestItemService.GetQuoteRequestItemByIdAsync(model.Id);

        if (requestItem == null)
            return RedirectToAction("Edit", "QCQuoteRequest", new { id = model.QuoteRequestId });

        await _quoteRequestItemService.DeleteQuoteRequestItemAsync(requestItem);
        return RedirectToAction("Edit", "QCQuoteRequest", new { id = model.QuoteRequestId });
    }

    #endregion
}
