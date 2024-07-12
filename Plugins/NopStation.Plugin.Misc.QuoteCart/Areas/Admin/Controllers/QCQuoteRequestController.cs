using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services;
using NopStation.Plugin.Misc.QuoteCart.Services.Email;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;
using NopStation.Plugin.Misc.QuoteCart.Services.RequestMessage;
using NUglify.Helpers;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Controllers;

public class QCQuoteRequestController : NopStationAdminController
{
    #region Fields 

    private readonly IQuoteRequestModelFactory _quoteRequestModelFactory;
    private readonly IQuoteRequestService _quoteRequestService;
    private readonly IQuoteFormService _quoteFormService;
    private readonly IAddressService _addressService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificationService _notificationService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IRequestProcessingService _requestProcessingService;
    private readonly ILocalizationService _localizationService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IQuoteRequestItemService _quoteRequestItemService;
    private readonly ICurrencyService _currencyService;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly ITaxService _taxService;
    private readonly IPermissionService _permissionService;
    private readonly IQuoteCartEmailService _quoteCartEmailService;
    private readonly IQuoteRequestMessageService _quoteRequestMessageService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;

    #endregion

    #region Ctor

    public QCQuoteRequestController(
        IQuoteRequestModelFactory quoteRequestModelFactory,
        IQuoteRequestService quoteRequestService,
        IQuoteFormService quoteCartFormService,
        IAddressService addressService,
        ICustomerActivityService customerActivityService,
        IEventPublisher eventPublisher,
        INotificationService notificationService,
        IProductAttributeFormatter productAttributeFormatter,
        IRequestProcessingService requestProcessingService,
        ILocalizationService localizationService,
        IGenericAttributeService genericAttributeService,
        IQuoteRequestItemService quoteRequestItemService,
        ICurrencyService currencyService,
        IProductService productService,
        IOrderService orderService,
        ICustomerService customerService,
        ITaxService taxService,
        IPermissionService permissionService,
        IQuoteCartEmailService quoteCartEmailService,
        IQuoteRequestMessageService quoteRequestMessageService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IStoreService storeService)
    {
        _quoteRequestModelFactory = quoteRequestModelFactory;
        _quoteRequestService = quoteRequestService;
        _quoteFormService = quoteCartFormService;
        _addressService = addressService;
        _customerActivityService = customerActivityService;
        _eventPublisher = eventPublisher;
        _notificationService = notificationService;
        _productAttributeFormatter = productAttributeFormatter;
        _requestProcessingService = requestProcessingService;
        _localizationService = localizationService;
        _genericAttributeService = genericAttributeService;
        _quoteRequestItemService = quoteRequestItemService;
        _currencyService = currencyService;
        _productService = productService;
        _orderService = orderService;
        _customerService = customerService;
        _taxService = taxService;
        _permissionService = permissionService;
        _quoteCartEmailService = quoteCartEmailService;
        _quoteRequestMessageService = quoteRequestMessageService;
        _workContext = workContext;
        _storeContext = storeContext;
        _storeService = storeService;
    }

    #endregion

    #region Methods

    #region Quote request

    public virtual async Task<IActionResult> List(List<int> requestStatuses = null)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        //prepare model
        var model = await _quoteRequestModelFactory.PrepareQuoteRequestSearchModelAsync(new QuoteRequestSearchModel
        {
            SearchRequestStatusIds = requestStatuses
        });

        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> List(QuoteRequestSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _quoteRequestModelFactory.PrepareQuoteRequestListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost, FormValueRequired("go-to-request-by-number"), ActionName(nameof(List))]
    public async Task<IActionResult> JumpToRequest(QuoteRequestSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        if (int.TryParse(searchModel.GoDirectlyToCustomRequestNumber, out var quoteRequestId))
        {
            var quoteRequest = await _quoteRequestService.GetQuoteRequestByIdAsync(quoteRequestId);
            if (quoteRequest != null)
                return RedirectToAction("Edit", "QCQuoteRequest", new { id = quoteRequest.Id });
        }

        if (Guid.TryParse(searchModel.GoDirectlyToCustomRequestNumber, out var quoteRequestGuid))
        {
            var quoteRequest = await _quoteRequestService.GetQuoteRequestByGuidAsync(quoteRequestGuid);
            if (quoteRequest != null)
                return RedirectToAction("Edit", "QCQuoteRequest", new { id = quoteRequest.Id });
        }

        return RedirectToAction("List");
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var quoteRequest = await _quoteRequestService.GetQuoteRequestByIdAsync(id);

        if (quoteRequest == null)
            return RedirectToAction("List");

        var model = await _quoteRequestModelFactory.PrepareQuoteRequestModelAsync(null, quoteRequest);
        return View(model);
    }


    [HttpPost, EditAccess, ActionName("Edit")]
    [FormValueRequired("btnSaveRequestStatus")]
    public virtual async Task<IActionResult> ChangeOrderStatus(int id, QuoteRequestDetailsModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        //try to get an quoteRequest with the specified id
        var quoteRequest = await _quoteRequestService.GetQuoteRequestByIdAsync(id);
        if (quoteRequest == null)
            return RedirectToAction("List");

        try
        {
            quoteRequest.RequestStatusId = model.RequestStatusId;
            await _quoteRequestService.UpdateQuoteRequestAsync(quoteRequest);


            await _requestProcessingService.SetRequestStatusAsync(quoteRequest, quoteRequest.RequestStatus, true);

            //prepare model
            model = await _quoteRequestModelFactory.PrepareQuoteRequestModelAsync(model, quoteRequest);
            return View(model);
        }
        catch (Exception exc)
        {
            //prepare model
            model = await _quoteRequestModelFactory.PrepareQuoteRequestModelAsync(model, quoteRequest);

            await _notificationService.ErrorNotificationAsync(exc);
            return View(model);
        }
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("cancelrequest")]
    public virtual async Task<IActionResult> CancelOrder(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        //try to get an order with the specified id
        var quoteRequest = await _quoteRequestService.GetQuoteRequestByIdAsync(id);
        if (quoteRequest == null)
            return RedirectToAction("List");

        try
        {
            await _requestProcessingService.CancelRequestAsync(quoteRequest, true);
            var model = await _quoteRequestModelFactory.PrepareQuoteRequestModelAsync(null, quoteRequest);
            return View(model);

        }
        catch (Exception exc)
        {
            var model = await _quoteRequestModelFactory.PrepareQuoteRequestModelAsync(null, quoteRequest);
            await _notificationService.ErrorNotificationAsync(exc);
            return View(model);

        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var request = await _quoteRequestService.GetQuoteRequestByIdAsync(id);

        if (request == null)
            return RedirectToAction("List");

        await _quoteRequestService.DeleteQuoteRequestAsync(request);
        return RedirectToAction("List");
    }

    [HttpPost]
    public virtual async Task<IActionResult> SendResponse(int requestId, string message)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return await AccessDeniedDataTablesJson();

        var request = await _quoteRequestService.GetQuoteRequestByIdAsync(requestId);
        var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);
        var store = await _storeContext.GetCurrentStoreAsync();
        var form = await _quoteFormService.GetFormByIdAsync(request.FormId);
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        if (request == null)
            return BadRequest(new { Result = false, Error = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.NotFound") });

        if (customer == null)
            return BadRequest(new { Result = false, Error = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.CustomerNotFound") });

        var quoteRequestMessage = new QuoteRequestMessage
        {
            QuoteRequestId = requestId,
            Content = message,
            StoreId = store.Id,
            CustomerId = currentCustomer.Id,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _quoteRequestMessageService.InsertQuoteRequestMessageAsync(quoteRequestMessage);

        if (form != null && form.SendEmailToCustomer)
        {
            var customerFullName = customer != null ? await _customerService.GetCustomerFullNameAsync(customer) : await _localizationService.GetResourceAsync("Customer.Guest");
            await _quoteCartEmailService.SendEmailAsync(
                request,
                QuoteRequestNotificationType.CustomerReplySent,
                customerFullName,
                string.IsNullOrEmpty(customer.Email) ? request.GuestEmail : customer.Email,
                message: message);
        }

        return Json(new { Result = true, Data = quoteRequestMessage });
    }

    #endregion

    #region Convert to order

    [HttpGet]
    public async Task<IActionResult> ConvertToOrder(int requestId)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var request = await _quoteRequestService.GetQuoteRequestByIdAsync(requestId);
        if (request == null)
            return RedirectToAction(nameof(List));

        var model = await _quoteRequestModelFactory.PrepareConvertToOrderModelAsync(new ConvertToOrderModel(), request);

        var (isInStock, errors) = await ValidateConvertToOrderRequest(request);

        if (!isInStock)
        {
            _notificationService.WarningNotification("Some products are out of stock. Please check the order details.");
            errors.ForEach(x => ModelState.AddModelError("", x));
        }

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ConvertToOrder(ConvertToOrderModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var request = await _quoteRequestService.GetQuoteRequestByIdAsync(model.QuoteRequestId);

        if (request == null)
            return RedirectToAction(nameof(List));

        var items = await _quoteRequestItemService.GetAllQuoteRequestItemsAsync(request.Id);

        if (!items.Any())
            return RedirectToAction(nameof(Edit), new { id = model.QuoteRequestId });

        var (isInStock, errors) = await ValidateConvertToOrderRequest(request);

        if (!isInStock)
        {
            _notificationService.WarningNotification("Some products are out of stock. Please check the order details.");
            errors.ForEach(x => ModelState.AddModelError("", x));
        }

        if (ModelState.IsValid)
        {
            var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);

            if (customer == null)
                return RedirectToAction(nameof(Edit), new { id = model.QuoteRequestId });

            Address billingAddress = null, shippingAddress = null;

            // add new billing address
            if (model.BillingAddressId == 0)
            {
                billingAddress = new Address
                {
                    FirstName = model.BillingAddress.FirstName,
                    LastName = model.BillingAddress.LastName,
                    Email = model.BillingAddress.Email,
                    Company = model.BillingAddress.Company,
                    CountryId = model.BillingAddress.CountryId,
                    StateProvinceId = model.BillingAddress.StateProvinceId,
                    City = model.BillingAddress.City,
                    Address1 = model.BillingAddress.Address1,
                    Address2 = model.BillingAddress.Address2,
                    ZipPostalCode = model.BillingAddress.ZipPostalCode,
                    PhoneNumber = model.BillingAddress.PhoneNumber,
                    FaxNumber = model.BillingAddress.FaxNumber,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _addressService.InsertAddressAsync(billingAddress);
                await _customerService.InsertCustomerAddressAsync(customer, billingAddress);
                model.BillingAddressId = billingAddress.Id;
            }

            if (!model.ShipToSameAddress && model.ShippingAddressId == 0)
            {
                shippingAddress = new Address
                {
                    FirstName = model.ShippingAddress.FirstName,
                    LastName = model.ShippingAddress.LastName,
                    Email = model.ShippingAddress.Email,
                    Company = model.ShippingAddress.Company,
                    CountryId = model.ShippingAddress.CountryId,
                    StateProvinceId = model.ShippingAddress.StateProvinceId,
                    City = model.ShippingAddress.City,
                    Address1 = model.ShippingAddress.Address1,
                    Address2 = model.ShippingAddress.Address2,
                    ZipPostalCode = model.ShippingAddress.ZipPostalCode,
                    PhoneNumber = model.ShippingAddress.PhoneNumber,
                    FaxNumber = model.ShippingAddress.FaxNumber,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _addressService.InsertAddressAsync(shippingAddress);
                await _customerService.InsertCustomerAddressAsync(customer, shippingAddress);
                model.ShippingAddressId = shippingAddress.Id;
            }

            var customerBillingAddress = await _customerService.GetCustomerAddressAsync(customer.Id, model.BillingAddressId);
            var customerShippingAddress = model.ShipToSameAddress ? customerBillingAddress : await _customerService.GetCustomerAddressAsync(customer.Id, model.ShippingAddressId);
            var customerCurrencyId = customer.CurrencyId;
            var customerCurrency = await _currencyService.GetCurrencyByIdAsync(customerCurrencyId ?? 0) ?? await _quoteRequestService.GetCustomerCurrencyAsync(customer, request.StoreId);
            var customerTaxDisplayTypeId = customer.TaxDisplayTypeId;

            var order = new Order()
            {
                CustomOrderNumber = request.Id.ToString(),
                OrderGuid = Guid.NewGuid(),
                StoreId = request.StoreId,
                CustomerId = request.CustomerId,
                OrderStatus = OrderStatus.Pending,
                BillingAddressId = customerBillingAddress.Id,
                ShippingAddressId = customerShippingAddress.Id,
                ShippingMethod = model.ShippingMethodId,
                PaymentMethodSystemName = model.PaymentMethodSystemName,
                ShippingStatus = ShippingStatus.NotYetShipped,
                PaymentStatus = model.MarkAsPaid ? PaymentStatus.Paid : PaymentStatus.Pending,
                CustomerCurrencyCode = customerCurrency?.CurrencyCode ?? string.Empty,
                CurrencyRate = customerCurrency?.Rate ?? 1,
                CustomerTaxDisplayTypeId = customerTaxDisplayTypeId ?? (int)TaxDisplayType.ExcludingTax,
                CreatedOnUtc = DateTime.UtcNow,
                ShippingRateComputationMethodSystemName = model.ShippingRateComputationMethodSystemName,
                PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFee,
                OrderShippingExclTax = model.OrderShippingFee,
            };

            (order.OrderShippingInclTax, _) = await _taxService.GetShippingPriceAsync(model.OrderShippingFee, customer);
            (order.PaymentMethodAdditionalFeeInclTax, _) = await _taxService.GetPaymentMethodAdditionalFeeAsync(model.PaymentMethodAdditionalFee, customer);

            if (model.MarkAsPaid)
                order.PaidDateUtc = DateTime.UtcNow;

            await _orderService.InsertOrderAsync(order);

            foreach (var item in items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var orderItem = new OrderItem
                {
                    AttributesXml = item.AttributesXml,
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    AttributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, item.AttributesXml),
                };
                (orderItem.PriceInclTax, orderItem.PriceExclTax) = await _quoteRequestService.GetRequestItemPriceAsync(item, customer, item.Quantity);
                (orderItem.UnitPriceExclTax, orderItem.UnitPriceInclTax) = await _quoteRequestService.GetRequestItemPriceAsync(item, customer, 1);

                await _orderService.InsertOrderItemAsync(orderItem);
            }

            (order.OrderSubtotalInclTax, order.OrderSubtotalExclTax) = await _quoteRequestService.GetRequestSubTotalAsync(request);
            var taxTotal = order.OrderSubtotalInclTax - order.OrderSubtotalExclTax;
            order.OrderTax = taxTotal;
            order.OrderTotal = order.OrderSubtotalExclTax + model.PaymentMethodAdditionalFee + model.OrderShippingFee + taxTotal;

            await _orderService.UpdateOrderAsync(order);

            var orderNote = new OrderNote
            {
                OrderId = order.Id,
                Note = $"Created order from Quote Request #{request.Id}",
                CreatedOnUtc = DateTime.UtcNow,
                DisplayToCustomer = true
            };

            await _orderService.InsertOrderNoteAsync(orderNote);

            //raise event
            await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertedToOrder.Success"));

            return RedirectToAction("Edit", "Order", new { Area = "Admin", id = order.Id });
        }

        // something is wrong with input
        model = await _quoteRequestModelFactory.PrepareConvertToOrderModelAsync(model, request);

        return View(model);
    }

    [HttpGet]
    public virtual async Task<IActionResult> ShippingOptions(int quoteRequestId, int shippingAddressId, string shippingRateComputationMethodSystemName = null)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return await AccessDeniedDataTablesJson();

        var quoteRequest = await _quoteRequestService.GetQuoteRequestByIdAsync(quoteRequestId) ?? throw new NopException("Request not found");

        var model = new List<SelectListItem>();
        await _quoteRequestModelFactory.PrepareShippingMethodsAsync(model, quoteRequest, shippingRateComputationMethodSystemName, shippingAddressId);
        return Json(new { data = model });
    }

    [HttpGet]
    public async Task<IActionResult> AddDiscountedPriceForItem(int quoteItemId, decimal discountedPrice)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return await AccessDeniedDataTablesJson();

        if (quoteItemId == 0)
            return Json(new { success = false, message = "Invalid Quote request item id." });

        var quoteItem = await _quoteRequestItemService.GetQuoteRequestItemByIdAsync(quoteItemId);

        if (quoteItem == null)
            return Json(new { success = false, message = "Quote request item not found." });

        quoteItem.DiscountedPrice = discountedPrice;

        await _quoteRequestItemService.UpdateQuoteRequestItemAsync(quoteItem);

        return Json(new { success = true, message = "Price updated!" });
    }

    #endregion

    #region Restore original

    [HttpPost]
    public virtual async Task<IActionResult> RestoreOriginal(int requestId)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var request = await _quoteRequestService.GetQuoteRequestByIdAsync(requestId);

        if (request == null)
            return RedirectToAction(nameof(List));

        if (string.IsNullOrEmpty(request.OriginalRequestItemsXml))
        {
            _notificationService.ErrorNotification("Could not restore to original!");
            return RedirectToAction(nameof(Edit), new { id = request.Id });
        }

        await _quoteRequestService.RestoreOriginalAsync(request);
        _notificationService.SuccessNotification("Request has been restored to original.");
        return RedirectToAction(nameof(Edit), new { id = request.Id });
    }


    #endregion

    #region Send quote

    [HttpPost, ParameterBasedOnFormName("store-owner", "toStoreOwner")]
    public virtual async Task<IActionResult> SendQuote(int requestId, bool toStoreOwner)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var request = await _quoteRequestService.GetQuoteRequestByIdAsync(requestId);

        if (request == null)
            return RedirectToAction(nameof(List));

        if (toStoreOwner)
        {
            await _quoteCartEmailService.SendEmailToStoreOwner(request, QuoteRequestNotificationType.QuoteOffer);
            _notificationService.SuccessNotification("The quote has been successfully sent to store owner.");
        }
        else
        {
            var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);
            var sendTo = string.IsNullOrWhiteSpace(customer?.Email) ? request.GuestEmail : customer.Email;
            var fullName = customer != null ? await _customerService.GetCustomerFullNameAsync(customer) : await _localizationService.GetResourceAsync("Customer.Guest");

            await _quoteCartEmailService.SendEmailAsync(request, QuoteRequestNotificationType.QuoteOffer, fullName, sendTo);
            _notificationService.SuccessNotification($"The quote has been successfully sent to {sendTo}");
        }

        return RedirectToAction(nameof(Edit), new { id = request.Id });
    }

    #endregion

    #region Export data 


    [HttpPost, ActionName("ExportExcel")]
    [FormValueRequired("exportexcel-all")]
    public virtual async Task<IActionResult> ExportExcelAll()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        ////load requests
        //var requests = await _quoteRequestService.GetAllQuoteRequestsAsync();

        ////ensure that we at least one order selected
        //if (!requests.Any())
        //{
        //    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.requests.NoRequests"));
        //    return RedirectToAction("List");
        //}

        //try
        //{
        //    var dataTable = await _quoteRequestService.MakeDataTable(requests.ToList());
        //    using var wb = new XLWorkbook();
        //    wb.Worksheets.Add(dataTable);
        //    using var stream = new MemoryStream();
        //    wb.SaveAs(stream);
        //    return File(stream.ToArray(), QuoteCartDefaults.RequestsExportedFileType, QuoteCartDefaults.RequestsExportedFileName);
        //}
        //catch (Exception exc)
        //{
        //    await _notificationService.ErrorNotificationAsync(exc);
        //    return RedirectToAction("List");
        //}
        return NotFound();
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
    {

        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest))
            return AccessDeniedView();

        var requests = new List<QuoteRequest>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            foreach (var id in ids)
            {
                var rqst = await _quoteRequestService.GetQuoteRequestByIdAsync(id);
                requests.Add(rqst);
            }
        }
        return NotFound();

        //try
        //{
        //    var dataTable = await _quoteRequestService.MakeDataTable(requests);
        //    using var wb = new XLWorkbook();
        //    wb.Worksheets.Add(dataTable);
        //    using var stream = new MemoryStream();
        //    wb.SaveAs(stream);
        //    return File(stream.ToArray(), QuoteCartDefaults.RequestsExportedFileType, QuoteCartDefaults.RequestsExportedFileName);
        //}
        //catch (Exception exc)
        //{
        //    await _notificationService.ErrorNotificationAsync(exc);
        //    return RedirectToAction("List");
        //}
    }

    #endregion

    #region Utilities

    private async Task<(bool isInStock, IList<string> errors)> ValidateConvertToOrderRequest(QuoteRequest quoteRequest)
    {
        var requestItems = await _quoteRequestItemService.GetAllQuoteRequestItemsAsync(quoteRequest.Id);

        var errors = new List<string>();
        var status = true;

        foreach (var item in requestItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null)
            {
                status = false;
                errors.Add($"Product with ID {item.ProductId} not found or deleted.");
            }
            if (await _productService.GetTotalStockQuantityAsync(product) < item.Quantity)
            {
                status = false;
                errors.Add($"The product '{product.Name}' is out of stock");
            }
        }

        return (status, errors);
    }

    #endregion

    #endregion
}
