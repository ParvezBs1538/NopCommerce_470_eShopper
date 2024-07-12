using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/giftcard/[action]")]
public partial class GiftCardApiController : BaseAdminApiController
{
    #region Fields

    private readonly CurrencySettings _currencySettings;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IGiftCardModelFactory _giftCardModelFactory;
    private readonly IGiftCardService _giftCardService;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly IOrderService _orderService;
    private readonly IPermissionService _permissionService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly LocalizationSettings _localizationSettings;

    #endregion

    #region Ctor

    public GiftCardApiController(CurrencySettings currencySettings,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        IDateTimeHelper dateTimeHelper,
        IGiftCardModelFactory giftCardModelFactory,
        IGiftCardService giftCardService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IOrderService orderService,
        IPermissionService permissionService,
        IPriceFormatter priceFormatter,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings)
    {
        _currencySettings = currencySettings;
        _currencyService = currencyService;
        _customerActivityService = customerActivityService;
        _dateTimeHelper = dateTimeHelper;
        _giftCardModelFactory = giftCardModelFactory;
        _giftCardService = giftCardService;
        _languageService = languageService;
        _localizationService = localizationService;
        _orderService = orderService;
        _permissionService = permissionService;
        _priceFormatter = priceFormatter;
        _workflowMessageService = workflowMessageService;
        _localizationSettings = localizationSettings;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _giftCardModelFactory.PrepareGiftCardSearchModelAsync(new GiftCardSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GiftCardList([FromBody] BaseQueryModel<GiftCardSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _giftCardModelFactory.PrepareGiftCardListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _giftCardModelFactory.PrepareGiftCardModelAsync(new GiftCardModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<GiftCardModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var giftCard = model.ToEntity<GiftCard>();
            giftCard.CreatedOnUtc = DateTime.UtcNow;
            await _giftCardService.InsertGiftCardAsync(giftCard);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewGiftCard",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewGiftCard"), giftCard.GiftCardCouponCode), giftCard);

            return Created(giftCard.Id, await _localizationService.GetResourceAsync("Admin.GiftCards.Added"));
        }

        //prepare model
        model = await _giftCardModelFactory.PrepareGiftCardModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
            return AdminApiAccessDenied();

        //try to get a gift card with the specified id
        var giftCard = await _giftCardService.GetGiftCardByIdAsync(id);
        if (giftCard == null)
            return NotFound("No gift card found with the specified id");

        //prepare model
        var model = await _giftCardModelFactory.PrepareGiftCardModelAsync(null, giftCard);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<GiftCardModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a gift card with the specified id
        var giftCard = await _giftCardService.GetGiftCardByIdAsync(model.Id);
        if (giftCard == null)
            return NotFound("No gift card found with the specified id");

        var order = await _orderService.GetOrderByOrderItemAsync(giftCard.PurchasedWithOrderItemId ?? 0);

        model.PurchasedWithOrderId = order?.Id;
        model.RemainingAmountStr = await _priceFormatter.FormatPriceAsync(await _giftCardService.GetGiftCardRemainingAmountAsync(giftCard), true, false);
        model.AmountStr = await _priceFormatter.FormatPriceAsync(giftCard.Amount, true, false);
        model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(giftCard.CreatedOnUtc, DateTimeKind.Utc);
        model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
        model.PurchasedWithOrderNumber = order?.CustomOrderNumber;

        if (ModelState.IsValid)
        {
            giftCard = model.ToEntity(giftCard);
            await _giftCardService.UpdateGiftCardAsync(giftCard);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditGiftCard",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditGiftCard"), giftCard.GiftCardCouponCode), giftCard);

            return Ok(await _localizationService.GetResourceAsync("Admin.GiftCards.Updated"));
        }

        //prepare model
        model = await _giftCardModelFactory.PrepareGiftCardModelAsync(model, giftCard, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost]
    public virtual IActionResult GenerateCouponCode()
    {
        return Ok(new GenericResponseModel<object>() { Data = new { CouponCode = _giftCardService.GenerateGiftCardCode() } });
    }

    [HttpPost]
    public virtual async Task<IActionResult> NotifyRecipient([FromBody] BaseQueryModel<GiftCardModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a gift card with the specified id
        var giftCard = await _giftCardService.GetGiftCardByIdAsync(model.Id);
        if (giftCard == null)
            return NotFound("No gift card found with the specified id");

        try
        {
            if (!CommonHelper.IsValidEmail(giftCard.RecipientEmail))
                return BadRequest("Recipient email is not valid");

            if (!CommonHelper.IsValidEmail(giftCard.SenderEmail))
                return BadRequest("Sender email is not valid");

            var languageId = 0;
            var order = await _orderService.GetOrderByOrderItemAsync(giftCard.PurchasedWithOrderItemId ?? 0);

            if (order != null)
            {
                var customerLang = await _languageService.GetLanguageByIdAsync(order.CustomerLanguageId);
                customerLang ??= (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
                if (customerLang != null)
                    languageId = customerLang.Id;
            }
            else
            {
                languageId = _localizationSettings.DefaultAdminLanguageId;
            }

            var queuedEmailIds = await _workflowMessageService.SendGiftCardNotificationAsync(giftCard, languageId);
            if (queuedEmailIds.Any())
            {
                giftCard.IsRecipientNotified = true;
                await _giftCardService.UpdateGiftCardAsync(giftCard);
                model.IsRecipientNotified = true;
            }

            return Ok(await _localizationService.GetResourceAsync("Admin.GiftCards.RecipientNotified"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
            return AdminApiAccessDenied();

        //try to get a gift card with the specified id
        var giftCard = await _giftCardService.GetGiftCardByIdAsync(id);
        if (giftCard == null)
            return NotFound("No gift card found with the specified id");

        await _giftCardService.DeleteGiftCardAsync(giftCard);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteGiftCard",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteGiftCard"), giftCard.GiftCardCouponCode), giftCard);

        return Ok(await _localizationService.GetResourceAsync("Admin.GiftCards.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> UsageHistoryList([FromBody] BaseQueryModel<GiftCardUsageHistorySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a gift card with the specified id
        var giftCard = await _giftCardService.GetGiftCardByIdAsync(searchModel.GiftCardId);
        if (giftCard == null)
            return NotFound("No gift card found with the specified id");

        //prepare model
        var model = await _giftCardModelFactory.PrepareGiftCardUsageHistoryListModelAsync(searchModel, giftCard);

        return OkWrap(model);
    }

    #endregion
}