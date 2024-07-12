using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Http.Extensions;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/newslettersubscription/[action]")]
public partial class NewsLetterSubscriptionApiController : BaseAdminApiController
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IExportManager _exportManager;
    private readonly IImportManager _importManager;
    private readonly ILocalizationService _localizationService;
    private readonly INewsletterSubscriptionModelFactory _newsletterSubscriptionModelFactory;
    private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public NewsLetterSubscriptionApiController(IDateTimeHelper dateTimeHelper,
        IExportManager exportManager,
        IImportManager importManager,
        ILocalizationService localizationService,
        INewsletterSubscriptionModelFactory newsletterSubscriptionModelFactory,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        IPermissionService permissionService)
    {
        _dateTimeHelper = dateTimeHelper;
        _exportManager = exportManager;
        _importManager = importManager;
        _localizationService = localizationService;
        _newsletterSubscriptionModelFactory = newsletterSubscriptionModelFactory;
        _newsLetterSubscriptionService = newsLetterSubscriptionService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _newsletterSubscriptionModelFactory.PrepareNewsletterSubscriptionSearchModelAsync(new NewsletterSubscriptionSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<NewsletterSubscriptionSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _newsletterSubscriptionModelFactory.PrepareNewsletterSubscriptionListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SubscriptionUpdate([FromBody] BaseQueryModel<NewsletterSubscriptionModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByIdAsync(model.Id);

        //fill entity from model
        subscription = model.ToEntity(subscription);
        await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(subscription);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> SubscriptionDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
            return AdminApiAccessDenied();

        var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByIdAsync(id);
        if (subscription == null)
            return NotFound("No subscription found with the specified id");

        await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(subscription);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportCsv([FromBody] BaseQueryModel<NewsletterSubscriptionSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        bool? isActive = null;
        if (model.ActiveId == 1)
            isActive = true;
        else if (model.ActiveId == 2)
            isActive = false;

        var startDateValue = model.StartDate == null ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var endDateValue = model.EndDate == null ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        var subscriptions = await _newsLetterSubscriptionService.GetAllNewsLetterSubscriptionsAsync(model.SearchEmail,
            startDateValue, endDateValue, model.StoreId, isActive, model.CustomerRoleId);

        var result = await _exportManager.ExportNewsletterSubscribersToTxtAsync(subscriptions);

        var fileName = $"newsletter_emails_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{CommonHelper.GenerateRandomDigitCode(4)}.csv";

        return File(Encoding.UTF8.GetBytes(result), MimeTypes.TextCsv, fileName);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ImportCsv()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
            return AdminApiAccessDenied();

        try
        {
            var importcsvfile = await Request.GetFirstOrDefaultFileAsync();
            if (importcsvfile != null && importcsvfile.Length > 0)
            {
                var count = await _importManager.ImportNewsletterSubscribersFromTxtAsync(importcsvfile.OpenReadStream());

                return Ok(string.Format(await _localizationService.GetResourceAsync("Admin.Promotions.NewsLetterSubscriptions.ImportEmailsSuccess"), count));
            }

            return BadRequest(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    #endregion
}