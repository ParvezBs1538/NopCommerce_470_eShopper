using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Routing;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Factories;
using NopStation.Plugin.Misc.QuoteCart.Models;
using NopStation.Plugin.Misc.QuoteCart.Services;
using NopStation.Plugin.Misc.QuoteCart.Services.Email;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;
using NopStation.Plugin.Misc.QuoteCart.Services.RequestMessage;

namespace NopStation.Plugin.Misc.QuoteCart.Controllers;

public class QuoteCartController : NopStationPublicController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IProductService _productService;
    private readonly IQuoteFormService _quoteFormService;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IQuoteCartService _quoteCartService;
    private readonly IWorkContext _workContext;
    private readonly QuoteCartSettings _quoteCartSettings;
    private readonly IStoreContext _storeContext;
    private readonly IQuoteCartModelFactory _quoteCartModelFactory;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IFormAttributeParser _formAttributeParser;
    private readonly IFormAttributeFormatter _formAttributeFormatter;
    private readonly IQuoteRequestService _quoteRequestService;
    private readonly IQuoteRequestItemService _quoteRequestItemService;
    private readonly INopFileProvider _fileProvider;
    private readonly IPublicQuoteRequestModelFactory _quoteRequestModelFactoryPublic;
    private readonly IRequestProcessingService _requestProcessingService;
    private readonly ICustomerService _customerService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly ILocalizationService _localizationService;
    private readonly IQuoteCartEmailService _quoteCartEmailService;
    private readonly IQuoteRequestMessageService _quoteRequestMessageService;
    private readonly IDownloadService _downloadService;
    private readonly IFormAttributeService _formAttributeService;
    private readonly INopUrlHelper _nopUrlHelper;


    #endregion

    #region Ctor

    public QuoteCartController(
        IPermissionService permissionService,
        IProductService productService,
        IQuoteFormService quoteFormService,
        IProductAttributeParser productAttributeParser,
        IUrlRecordService urlRecordService,
        IQuoteCartService quoteCartService,
        IWorkContext workContext,
        QuoteCartSettings quoteCartSettings,
        IStoreContext storeContext,
        IQuoteCartModelFactory quoteCartModelFactory,
        IStoreMappingService storeMappingService,
        IFormAttributeParser formAttributeParser,
        IFormAttributeFormatter formAttributeFormatter,
        IQuoteRequestService quoteRequestService,
        IQuoteRequestItemService quoteRequestItemService,
        INopFileProvider fileProvider,
        IPublicQuoteRequestModelFactory quoteRequestModelFactoryPublic,
        IRequestProcessingService requestProcessingService,
        ICustomerService customerService,
        IEmailAccountService emailAccountService,
        ILocalizationService localizationService,
        IQuoteCartEmailService quoteCartEmailService,
        IQuoteRequestMessageService quoteRequestMessageService,
        IDownloadService downloadService,
        IFormAttributeService formAttributeService,
        INopUrlHelper nopUrlHelper)
    {
        _permissionService = permissionService;
        _productService = productService;
        _quoteFormService = quoteFormService;
        _productAttributeParser = productAttributeParser;
        _urlRecordService = urlRecordService;
        _quoteCartService = quoteCartService;
        _workContext = workContext;
        _quoteCartSettings = quoteCartSettings;
        _storeContext = storeContext;
        _quoteCartModelFactory = quoteCartModelFactory;
        _storeMappingService = storeMappingService;
        _formAttributeParser = formAttributeParser;
        _formAttributeFormatter = formAttributeFormatter;
        _quoteRequestService = quoteRequestService;
        _quoteRequestItemService = quoteRequestItemService;
        _fileProvider = fileProvider;
        _quoteRequestModelFactoryPublic = quoteRequestModelFactoryPublic;
        _requestProcessingService = requestProcessingService;
        _customerService = customerService;
        _emailAccountService = emailAccountService;
        _localizationService = localizationService;
        _quoteCartEmailService = quoteCartEmailService;
        _quoteRequestMessageService = quoteRequestMessageService;
        _downloadService = downloadService;
        _formAttributeService = formAttributeService;
        _nopUrlHelper = nopUrlHelper;
    }

    #endregion

    #region Methods

    #region Public cart

    [HttpGet]
    public async Task<IActionResult> Cart()
    {
        if (!_quoteCartSettings.EnableQuoteCart || !await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.SendQuoteRequest))
            return AccessDeniedView();

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _quoteCartService.GetQuoteCartAsync(await _workContext.GetCurrentCustomerAsync(), store.Id);

        var forms = await _quoteFormService.GetActiveFormsAsync();
        var model = await _quoteCartModelFactory.PrepareQuoteCartModelAsync(new CartModel(), cart, forms);

        return View(model);
    }

    [HttpPost]
    [FormValueRequired("updatecart")]
    public async Task<IActionResult> UpdateCart(IFormCollection form)
    {
        if (!_quoteCartSettings.EnableQuoteCart || !await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.SendQuoteRequest))
            return AccessDeniedView();

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _quoteCartService.GetQuoteCartAsync(await _workContext.GetCurrentCustomerAsync(), store.Id);

        var products = (await _productService.GetProductsByIdsAsync(cart.Select(item => item.ProductId).Distinct().ToArray()))
           .ToDictionary(item => item.Id, item => item);

        var itemsWithNewQuantity = cart.Select(item => new
        {
            NewQuantity = int.TryParse(form[$"itemquantity{item.Id}"], out var quantity) ? quantity : item.Quantity,
            Item = item,
            Product = products.TryGetValue(item.ProductId, out var value) ? value : null
        }).Where(item => item.NewQuantity != item.Item.Quantity).ToList();

        foreach (var item in itemsWithNewQuantity)
        {
            var qci = item.Item;
            qci.Quantity = item.NewQuantity;
            await _quoteCartService.UpdateQuoteCartAsync(qci);
        }

        return RedirectToAction("Cart");
    }

    public async Task<IActionResult> AddQuoteItem(int productId, int quantity = 1, string attributeXml = "")
    {
        if (!_quoteCartSettings.EnableQuoteCart || !await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.SendQuoteRequest))
            return await AccessDeniedDataTablesJson();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
            return BadRequest(new { message = "The product does not exist." });

        var redirectUrl = await _nopUrlHelper.RouteGenericUrlAsync<Nop.Core.Domain.Catalog.Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) });

        if (product.IsRental)
        {
            return Json(new { redirect = redirectUrl });
        }

        var warnings = await _quoteCartService.AddToQuoteCartAsync(customer, product, store.Id, attributeXml, quantity);

        if (warnings.Any())
            return BadRequest(new { message = string.Join(", ", warnings) });

        var cartItems = await _quoteCartService.GetQuoteCartAsync(customer);

        return Json(new { itemTotal = cartItems.Sum(item => item.Quantity) });
    }

    [HttpPost]
    public async Task<IActionResult> AddQuoteItem_Details(int productId, IFormCollection form)
    {
        if (!_quoteCartSettings.EnableQuoteCart || !await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.SendQuoteRequest))
            return await AccessDeniedDataTablesJson();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
            return BadRequest(new { message = "The product does not exist." });

        var warnings = new List<string>();

        //entered quantity
        var quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

        _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

        //product and gift card attributes
        var attributes = await _productAttributeParser.ParseProductAttributesAsync(product, form, warnings);

        warnings.AddRange(await _quoteCartService.AddToQuoteCartAsync(customer, product, store.Id, attributes, quantity, rentalStartDate, rentalEndDate));

        if (warnings.Count != 0)
            return BadRequest(new { message = string.Join(", ", warnings) });

        var cartItems = await _quoteCartService.GetQuoteCartAsync(customer);

        return Json(new { itemTotal = cartItems.Sum(item => item.Quantity) });
    }

    public async Task<IActionResult> RemoveQuoteItem(int itemId)
    {
        await _quoteCartService.DeleteFromQuoteCartAsync(itemId);

        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> ClearCart()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        await _quoteCartService.ClearCartAsync(await _workContext.GetCurrentCustomerAsync(), store.Id);

        return RedirectToAction(nameof(Cart));
    }

    #endregion

    #region Quote response

    [HttpPost]
    public virtual async Task<IActionResult> AddQuoteRequest(int formId, IFormCollection form)
    {
        var errorMessage = "";
        var quoteForm = await _quoteFormService.GetFormByIdAsync(formId);
        var store = await _storeContext.GetCurrentStoreAsync();
        if (quoteForm == null || quoteForm.Deleted)
            return InvokeHttp404();


        var notAvailable =
            //published?
            !quoteForm.Active ||
             quoteForm.Deleted ||
            //Store mapping
            !await _storeMappingService.AuthorizeAsync(quoteForm);

        var customer = await _workContext.GetCurrentCustomerAsync();

        var (attributesXml, _) = await _formAttributeParser.ParseFormAttributesAsync(quoteForm, form);

        var warnings = await _formAttributeParser.GetFormAttributeWarningsAsync(quoteForm, attributesXml);
        if (warnings.Count != 0)
        {
            errorMessage += string.Join(", ", warnings);
        }

        string guestEmail = null;

        if (await _customerService.IsGuestAsync(customer))
        {
            if (form.TryGetValue("GuestEmail", out var ge) && !string.IsNullOrEmpty(ge))
                guestEmail = ge.ToString();
            else
                errorMessage += await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.GuestEmail.Required");
        }

        var quoteCartItems = await _quoteCartService.GetQuoteCartAsync(customer, store.Id);

        if (_quoteCartSettings.MaxQuoteItemCount > 0 && quoteCartItems.Count > _quoteCartSettings.MaxQuoteItemCount)
        {
            errorMessage += string.Format(await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.QuoteRequests.ItemLimitExceeded"), _quoteCartSettings.MaxQuoteItemCount);
        }

        if (!string.IsNullOrEmpty(errorMessage))
            return Json(new { success = false, message = errorMessage });

        if (ModelState.IsValid)
        {
            var quoteRequest = new QuoteRequest()
            {
                GuestEmail = guestEmail,
                AttributeXml = attributesXml,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                FormId = quoteForm.Id,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                RequestId = Guid.NewGuid(),
                RequestStatus = RequestStatus.Pending,
                OriginalRequestItemsXml = attributesXml,
            };

            var quoteRequestItems = new List<QuoteRequestItem>();

            foreach (var item in quoteCartItems)
            {
                var quoteRequestItem = new QuoteRequestItem()
                {
                    QuoteRequestId = quoteRequest.Id,
                    StoreId = store.Id,
                    CustomerId = customer.Id,
                    AttributesXml = item.AttributesXml,
                    CreatedOnUtc = DateTime.UtcNow,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId,
                    RentalStartDateUtc = item.RentalStartDateUtc,
                    RentalEndDateUtc = item.RentalEndDateUtc,
                };
                quoteRequestItems.Add(quoteRequestItem);
            }

            quoteRequest.OriginalRequestItemsXml = await _quoteCartService.ExportQuoteRequestItemsXmlAsync(quoteRequestItems);
            await _quoteRequestService.InsertQuoteRequestAsync(quoteRequest);

            quoteRequestItems.ForEach(x => x.QuoteRequestId = quoteRequest.Id);
            await _quoteRequestItemService.InsertQuoteRequestItemsAsync(quoteRequestItems);

            if (_quoteCartSettings.ClearCartAfterSubmission)
                await _quoteCartService.ClearCartAsync(customer, store.Id);

            if (quoteForm != null && quoteForm.SendEmailToStoreOwner)
            {
                await _quoteCartEmailService.SendEmailToStoreOwner(
                    quoteRequest,
                    QuoteRequestNotificationType.StoreRequestSubmitted);
            }
            if (quoteForm != null && quoteForm.SendEmailToCustomer && (!string.IsNullOrEmpty(customer.Email) || !string.IsNullOrEmpty(guestEmail)))
            {
                var sendTo = string.IsNullOrWhiteSpace(customer?.Email) ? quoteRequest.GuestEmail : customer.Email;
                var fullName = customer != null ? await _customerService.GetCustomerFullNameAsync(customer) : await _localizationService.GetResourceAsync("Customer.Guest");
                await _quoteCartEmailService.SendEmailAsync(quoteRequest, QuoteRequestNotificationType.CustomerRequestSubmitted, fullName, sendTo);
            }
            return Json(new {
                success = true,
                redirect = Url.Action("RequestSuccess", "QuoteRequest",
                new {
                    requestId = quoteRequest.Id
                })
            });
        }

        return Json(new { success = false, message = errorMessage });
    }


    [HttpPost]
    public virtual async Task<IActionResult> QuoteForm_AttributeChange(int formId, IFormCollection form)
    {
        var quoteForm = await _quoteFormService.GetFormByIdAsync(formId);
        var enabledAttributeMappingIds = new List<int>();
        var disabledAttributeMappingIds = new List<int>();

        if (quoteForm == null)
        {
            var message = new[] { "Form not found" };
            return Ok(new
            {
                enabledAttributeMappingIds,
                disabledAttributeMappingIds,
                message
            });
        }

        var errors = new List<string>();
        var (attributeXml, _) = await _formAttributeParser.ParseFormAttributesAsync(quoteForm, form);

        var attributes = await _formAttributeService.GetFormAttributeMappingsByQuoteFormIdAsync(quoteForm.Id);
        foreach (var attribute in attributes)
        {
            var conditionMet = await _formAttributeParser.IsConditionMetAsync(attribute, attributeXml);
            if (!conditionMet.HasValue)
                continue;

            if (conditionMet.Value)
                enabledAttributeMappingIds.Add(attribute.Id);
            else
                disabledAttributeMappingIds.Add(attribute.Id);
        }

        return Ok(new
        {
            enabledAttributeMappingIds,
            disabledAttributeMappingIds,
            message = errors.Count != 0 ? errors.ToArray() : null
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> UploadFileFormAttribute(int attributeId)
    {
        var attribute = await _formAttributeService.GetFormAttributeMappingByIdAsync(attributeId);
        if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
        {
            return Json(new
            {
                success = false,
                downloadGuid = Guid.Empty
            });
        }

        if (Request.Form.Files.Count <= 0)
        {
            return Json(new
            {
                success = false,
                message = "No file uploaded",
                downloadGuid = Guid.Empty
            });
        }

        var httpPostedFile = Request.Form.Files[0];

        var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

        var qqFileNameParameter = "qqfilename";
        var fileName = httpPostedFile.FileName;
        if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
            fileName = Request.Form[qqFileNameParameter].ToString();
        //remove path (passed in IE)
        fileName = _fileProvider.GetFileName(fileName);

        var contentType = httpPostedFile.ContentType;

        var fileExtension = _fileProvider.GetFileExtension(fileName);
        if (!string.IsNullOrEmpty(fileExtension))
            fileExtension = fileExtension.ToLowerInvariant();

        if (attribute.ValidationFileMaximumSize.HasValue)
        {
            //compare in bytes
            var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
            if (fileBinary.Length > maxFileSizeBytes)
            {
                //when returning JSON the mime-type must be set to text/plain
                //otherwise some browsers will pop-up a "Save As" dialog.
                return Json(new
                {
                    success = false,
                    message = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                    downloadGuid = Guid.Empty
                });
            }
        }

        var download = new Download
        {
            DownloadGuid = Guid.NewGuid(),
            UseDownloadUrl = false,
            DownloadUrl = string.Empty,
            DownloadBinary = fileBinary,
            ContentType = contentType,
            //we store filename without extension for downloads
            Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
            Extension = fileExtension,
            IsNew = true
        };
        await _downloadService.InsertDownloadAsync(download);

        //when returning JSON the mime-type must be set to text/plain
        //otherwise some browsers will pop-up a "Save As" dialog.
        return Json(new
        {
            success = true,
            message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
            downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
            downloadGuid = download.DownloadGuid
        });
    }

    #endregion

    #endregion
}