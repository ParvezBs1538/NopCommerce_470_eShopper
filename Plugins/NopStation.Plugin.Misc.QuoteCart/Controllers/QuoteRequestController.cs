using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Factories;
using NopStation.Plugin.Misc.QuoteCart.Models;
using NopStation.Plugin.Misc.QuoteCart.Services;
using NopStation.Plugin.Misc.QuoteCart.Services.Email;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;
using NopStation.Plugin.Misc.QuoteCart.Services.RequestMessage;

namespace NopStation.Plugin.Misc.QuoteCart.Controllers;

public class QuoteRequestController : NopStationPublicController
{
    #region Fields

    private readonly IQuoteRequestService _quoteRequestService;
    private readonly IQuoteRequestItemService _quoteRequestItemService;
    private readonly IWorkContext _workContext;
    private readonly QuoteCartSettings _quoteCartSettings;
    private readonly IBBCodeHelper _bbCodeHelper;
    private readonly IStoreContext _storeContext;
    private readonly INopFileProvider _fileProvider;
    private readonly IPublicQuoteRequestModelFactory _quoteRequestModelFactoryPublic;
    private readonly IRequestProcessingService _requestProcessingService;
    private readonly ICustomerService _customerService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly ILocalizationService _localizationService;
    private readonly IQuoteFormService _quoteFormService;
    private readonly IQuoteCartService _quoteCartService;
    private readonly IQuoteCartEmailService _quoteCartEmailService;
    private readonly IQuoteRequestMessageService _quoteRequestMessageService;
    private readonly IFormAttributeParser _formAttributeParser;
    private readonly IFormAttributeFormatter _formAttributeFormatter;
    private readonly IStoreMappingService _storeMappingService;
    //private readonly IQuoteForm _storeMappingService;

    #endregion

    #region Ctor 

    public QuoteRequestController(
        ICustomerService customerService,
        IEmailAccountService emailAccountService,
        ILocalizationService localizationService,
        INopFileProvider fileProvider,
        IQuoteCartEmailService quoteCartEmailService,
        IQuoteFormService quoteCartFormService,
        IQuoteCartService quoteCartService,
        IQuoteRequestItemService quoteRequestItemService,
        IQuoteRequestMessageService quoteRequestMessageService,
        IPublicQuoteRequestModelFactory quoteRequestModelFactoryPublic,
        IQuoteRequestService quoteRequestService,
        IRequestProcessingService requestProcessingService,
        IStoreContext storeContext,
        IWorkContext workContext,
        QuoteCartSettings quoteCartSettings,
        IBBCodeHelper bbCodeHelper,
        IFormAttributeParser formAttributeParser,
        IFormAttributeFormatter formAttributeFormatter,
        IStoreMappingService storeMappingService)
    {
        _quoteRequestService = quoteRequestService;
        _quoteRequestItemService = quoteRequestItemService;
        _workContext = workContext;
        _quoteCartSettings = quoteCartSettings;
        _bbCodeHelper = bbCodeHelper;
        _storeContext = storeContext;
        _fileProvider = fileProvider;
        _quoteRequestModelFactoryPublic = quoteRequestModelFactoryPublic;
        _requestProcessingService = requestProcessingService;
        _customerService = customerService;
        _emailAccountService = emailAccountService;
        _localizationService = localizationService;
        _quoteFormService = quoteCartFormService;
        _quoteCartService = quoteCartService;
        _quoteCartEmailService = quoteCartEmailService;
        _quoteRequestMessageService = quoteRequestMessageService;
        _formAttributeParser = formAttributeParser;
        _formAttributeFormatter = formAttributeFormatter;
        _storeMappingService = storeMappingService;
    }

    #endregion

    #region Methods 

    [HttpPost]
    public async Task<IActionResult> AddQuoteRequest(int formId)
    {
        var quoteForm = await _quoteFormService.GetFormByIdAsync(formId);

        if (quoteForm == null)
            return Json(new { success = false, message = "Quote form not found" });

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var quoteCartItems = await _quoteCartService.GetQuoteCartAsync(customer, store.Id);

        if (_quoteCartSettings.MaxQuoteItemCount > 0 && quoteCartItems.Count > _quoteCartSettings.MaxQuoteItemCount)
        {
            var errorMessage = string.Format(await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.QuoteRequests.ItemLimitExceeded"), _quoteCartSettings.MaxQuoteItemCount);
            return Json(new { success = false, message = errorMessage });
        }

        var quoteRequest = new QuoteRequest()
        {
            CustomerId = customer.Id,
            StoreId = store.Id,
            CreatedOnUtc = DateTime.UtcNow,
            RequestId = Guid.NewGuid(),
            FormId = formId,
            RequestStatus = RequestStatus.Pending
        };
        await _quoteRequestService.InsertQuoteRequestAsync(quoteRequest);

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
                RentalStartDateUtc = item.RentalEndDateUtc,
                RentalEndDateUtc = item.RentalEndDateUtc,
            };
            quoteRequestItems.Add(quoteRequestItem);
        }

        quoteRequest.OriginalRequestItemsXml = await _quoteCartService.ExportQuoteRequestItemsXmlAsync(quoteRequestItems);
        await _quoteRequestItemService.InsertQuoteRequestItemsAsync(quoteRequestItems);

        if (_quoteCartSettings.ClearCartAfterSubmission)
            await _quoteCartService.ClearCartAsync(customer, store.Id);

        if (quoteForm != null && quoteForm.SendEmailToStoreOwner)
        {
            var storeOwnerEmail = await _emailAccountService.GetEmailAccountByIdAsync(quoteForm.DefaultEmailAccountId);
            await _quoteCartEmailService.SendEmailAsync(
                quoteRequest,
                QuoteRequestNotificationType.StoreRequestSubmitted,
                store.Name,
                storeOwnerEmail.Email);
        }

        if (quoteForm != null && quoteForm.SendEmailToCustomer)
        {
            var sendTo = string.IsNullOrWhiteSpace(customer?.Email) ? quoteRequest.GuestEmail : customer.Email;
            var fullName = customer != null ? await _customerService.GetCustomerFullNameAsync(customer) : await _localizationService.GetResourceAsync("Customer.Guest");
            await _quoteCartEmailService.SendEmailAsync(quoteRequest, QuoteRequestNotificationType.CustomerRequestSubmitted, fullName, sendTo);
        }

        return Json(new { success = true, redirect = Url.Action(nameof(RequestSuccess), "QuoteRequest", new { requestId = quoteRequest.Id }) });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> AttachmentUpload(IFormFile qqfile, string qqfilename)
    {
        var file = qqfile;
        ArgumentNullException.ThrowIfNull(qqfile);

        var attachmentRootPath = _fileProvider.GetAbsolutePath(string.Format(QuoteCartDefaults.QuoteCartAttachmentsPath, await _storeContext.GetActiveStoreScopeConfigurationAsync()));

        if (!_fileProvider.DirectoryExists(attachmentRootPath))
        {
            _fileProvider.CreateDirectory(attachmentRootPath);
        }

        var attachmentPath = _fileProvider.Combine(attachmentRootPath, file.FileName);
        using var fileStream = new FileStream(attachmentPath, FileMode.Create);
        file.CopyTo(fileStream);

        return Json(new { success = true, message = "Uploaded" });
    }

    public async Task<IActionResult> AllRequest(int? pageNumber)
    {
        var model = await _quoteRequestModelFactoryPublic.PrepareRequestListModelAsync(pageNumber);

        return View(model);
    }

    public async Task<IActionResult> Details(Guid requestId)
    {
        var quoteRequest = await _quoteRequestService.GetQuoteRequestByGuidAsync(requestId);
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (!await _customerService.IsGuestAsync(customer) && customer.Id != quoteRequest.CustomerId && string.IsNullOrEmpty(quoteRequest.GuestEmail))
            return Challenge();

        if (await _customerService.IsGuestAsync(customer) && string.IsNullOrEmpty(quoteRequest.GuestEmail))
            return Challenge();

        //if(!string.IsNullOrEmpty(quoteRequest.GuestEmail) && cus)
        if (quoteRequest == null)
            return NotFound();

        var model = await _quoteRequestModelFactoryPublic.PrepareRequestDetailsModelAsync(new(), quoteRequest, true);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SendResponse(QuoteRequestDetailsModel model)
    {
        var request = await _quoteRequestService.GetQuoteRequestByIdAsync(model.Id);
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (request == null || customer == null)
            return NotFound();

        if (request.CustomerId != customer.Id)
            return AccessDeniedView();

        if (string.IsNullOrWhiteSpace(model.ResponseMessage))
            return Json(new { success = false, message = "Response message is required" });

        var store = await _storeContext.GetCurrentStoreAsync();
        var quoteRequestMessage = new QuoteRequestMessage
        {
            QuoteRequestId = request.Id,
            Content = _bbCodeHelper.FormatText(model.ResponseMessage, true, true, true, true, true, true, true),
            StoreId = store.Id,
            CustomerId = customer.Id,
            CreatedOnUtc = DateTime.UtcNow,
            Subject = "Quote message by " + customer?.Email ?? model?.GuestEmail
        };

        await _quoteRequestMessageService.InsertQuoteRequestMessageAsync(quoteRequestMessage);

        var form = await _quoteFormService.GetFormByIdAsync(request.FormId);

        if (form != null && form.SendEmailToStoreOwner)
        {
            await _quoteCartEmailService
                .SendEmailToStoreOwner(request, QuoteRequestNotificationType.StoreReplySent, model.ResponseMessage);
        }

        ViewData["NewResponse"] = true;

        return RedirectToAction(nameof(Details), new { requestId = request.RequestId });
    }

    public async Task<IActionResult> RequestSuccess(int requestId)
    {
        var request = await _quoteRequestService.GetQuoteRequestByIdAsync(requestId);
        if (request == null)
            return RedirectToAction("Cart", "QuoteCart");

        var model = request.ToModel<QuoteRequestModel>();

        return View(model);
    }

    #endregion
}
