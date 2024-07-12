using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Common;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models.Api;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/common/[action]")]
public partial class CommonApiController : BaseAdminApiController
{
    #region Const

    private const string EXPORT_IMPORT_PATH = @"files\exportimport";

    #endregion

    #region Fields

    private readonly ICommonModelFactory _commonModelFactory;
    private readonly ICustomerService _customerService;
    private readonly INopDataProvider _dataProvider;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly IMaintenanceService _maintenanceService;
    private readonly INopFileProvider _fileProvider;
    private readonly IPermissionService _permissionService;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;
    private readonly ILogger _logger;
    private readonly IAdminApiSiteMapModelFactory _adminApiSiteMapModelFactory;

    #endregion

    #region Ctor

    public CommonApiController(ICommonModelFactory commonModelFactory,
        ICustomerService customerService,
        INopDataProvider dataProvider,
        IDateTimeHelper dateTimeHelper,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IMaintenanceService maintenanceService,
        INopFileProvider fileProvider,
        IPermissionService permissionService,
        IQueuedEmailService queuedEmailService,
        IShoppingCartService shoppingCartService,
        IStaticCacheManager staticCacheManager,
        IUrlRecordService urlRecordService,
        IWebHelper webHelper,
        IWorkContext workContext,
        ILogger logger,
        IAdminApiSiteMapModelFactory adminApiSiteMapModelFactory)
    {
        _commonModelFactory = commonModelFactory;
        _customerService = customerService;
        _dataProvider = dataProvider;
        _dateTimeHelper = dateTimeHelper;
        _languageService = languageService;
        _localizationService = localizationService;
        _maintenanceService = maintenanceService;
        _fileProvider = fileProvider;
        _permissionService = permissionService;
        _queuedEmailService = queuedEmailService;
        _shoppingCartService = shoppingCartService;
        _staticCacheManager = staticCacheManager;
        _urlRecordService = urlRecordService;
        _webHelper = webHelper;
        _workContext = workContext;
        _logger = logger;
        _adminApiSiteMapModelFactory = adminApiSiteMapModelFactory;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> GetLocales()
    {
        var availableLanguages = await _languageService.GetAllLanguagesAsync(true);
        var model = new List<LocaleModel>();
        foreach (var language in availableLanguages)
        {
            model.Add(new LocaleModel()
            {
                Id = language.Id,
                Name = language.Name,
                FlagImageFileUrl = Url.Content("~/images/flags/" + language.FlagImageFileName)
            });
        }

        var responseModel = new GenericResponseModel<List<LocaleModel>>();
        responseModel.Data = model;

        return Ok(responseModel);
    }

    [HttpGet("{languageId?}")]
    public virtual async Task<IActionResult> GetStringResources(int? languageId = null)
    {
        Language lang = null;
        if (languageId.HasValue)
        {
            lang = await _languageService.GetLanguageByIdAsync(languageId.Value);
            if (!lang?.Published ?? false)
                languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
        }
        else
            languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        lang ??= await _languageService.GetLanguageByIdAsync(languageId.Value);
        var stringResources = await _localizationService.GetAllResourceValuesAsync(languageId.Value, false);

        var response = new LanguageResourceModel
        {
            StringResources = stringResources.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.Value)).ToList(),
            Rtl = lang.Rtl,
            LanguageNavSelector = await _commonModelFactory.PrepareLanguageSelectorModelAsync()
        };
        return OkWrap(response);
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetSiteMap()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AdminApiAccessDenied();

        var siteMap = await _adminApiSiteMapModelFactory.LoadFromAsync("~/Plugins/NopStation.Plugin.Misc.AdminApi/sitemap.config");

        var response = new GenericResponseModel<ApiSiteMapNodeModel>
        {
            Data = siteMap
        };
        return Ok(response);
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetCommonStatistics()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) ||
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) ||
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests) ||
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        {
            return AdminApiAccessDenied();
        }

        //a vendor doesn't have access to this report
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return Content(string.Empty);

        //prepare model
        var model = await _commonModelFactory.PrepareCommonStatisticsModelAsync();

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> SystemInfo()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _commonModelFactory.PrepareSystemInfoModelAsync(new SystemInfoModel());

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Warnings()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _commonModelFactory.PrepareSystemWarningModelsAsync();

        return Ok(model.ToGenericResponse());
    }

    public virtual async Task<IActionResult> Maintenance()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _commonModelFactory.PrepareMaintenanceModelAsync(new MaintenanceModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MaintenanceDeleteGuests([FromBody] BaseQueryModel<MaintenanceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var startDateValue = model.DeleteGuests.StartDate == null ? null
                        : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteGuests.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

        var endDateValue = model.DeleteGuests.EndDate == null ? null
                        : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteGuests.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        model.DeleteGuests.NumberOfDeletedCustomers = await _customerService.DeleteGuestCustomersAsync(startDateValue, endDateValue, model.DeleteGuests.OnlyWithoutShoppingCart);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MaintenanceDeleteAbandonedCarts([FromBody] BaseQueryModel<MaintenanceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var olderThanDateValue = _dateTimeHelper.ConvertToUtcTime(model.DeleteAbandonedCarts.OlderThan, await _dateTimeHelper.GetCurrentTimeZoneAsync());

        model.DeleteAbandonedCarts.NumberOfDeletedItems = await _shoppingCartService.DeleteExpiredShoppingCartItemsAsync(olderThanDateValue);
        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MaintenanceDeleteFiles([FromBody] BaseQueryModel<MaintenanceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var startDateValue = model.DeleteExportedFiles.StartDate == null ? null
                        : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteExportedFiles.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

        var endDateValue = model.DeleteExportedFiles.EndDate == null ? null
                        : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteExportedFiles.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        model.DeleteExportedFiles.NumberOfDeletedFiles = 0;

        foreach (var fullPath in _fileProvider.GetFiles(_fileProvider.GetAbsolutePath(EXPORT_IMPORT_PATH)))
        {
            try
            {
                var fileName = _fileProvider.GetFileName(fullPath);
                if (fileName.Equals("index.htm", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var info = _fileProvider.GetFileInfo(fullPath);
                var lastModifiedTimeUtc = info.LastModified.UtcDateTime;
                if ((!startDateValue.HasValue || startDateValue.Value < lastModifiedTimeUtc) &&
                    (!endDateValue.HasValue || lastModifiedTimeUtc < endDateValue.Value))
                {
                    _fileProvider.DeleteFile(fullPath);
                    model.DeleteExportedFiles.NumberOfDeletedFiles++;
                }
            }
            catch (Exception exc)
            {
                return InternalServerError(exc.Message);
            }
        }

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MaintenanceDeleteMinificationFiles([FromBody] BaseQueryModel<MaintenanceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        model.DeleteMinificationFiles.NumberOfDeletedFiles = 0;

        var errorList = new List<string>();
        foreach (var fullPath in _fileProvider.GetFiles(_fileProvider.GetAbsolutePath("bundles")))
        {
            try
            {
                var info = _fileProvider.GetFileInfo(fullPath);

                if (info.Name.Equals("index.htm", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                _fileProvider.DeleteFile(info.PhysicalPath);
                model.DeleteMinificationFiles.NumberOfDeletedFiles++;

            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(exc.Message, exc);
                errorList.Add(exc.Message);
            }
        }

        return errorList.Count > 0
            ? BadRequestWrap(model, null, errorList)
            : OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> BackupFiles([FromBody] BaseQueryModel<BackupFileSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _commonModelFactory.PrepareBackupFileListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> BackupDatabase([FromBody] BaseQueryModel<MaintenanceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        try
        {
            await _dataProvider.BackupDatabaseAsync(_maintenanceService.CreateNewBackupFilePath());
            return Ok(await _localizationService.GetResourceAsync("Admin.System.Maintenance.BackupDatabase.BackupCreated"));
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc);
            return BadRequest(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReIndexTables([FromBody] BaseQueryModel<MaintenanceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        try
        {
            await _dataProvider.ReIndexTablesAsync();
            return Ok(await _localizationService.GetResourceAsync("Admin.System.Maintenance.ReIndexTables.Complete"));
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc);
            return BadRequest(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> BackupAction([FromBody] BaseQueryModel<MaintenanceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var form = queryModel.FormValues.ToNameValueCollection();
        var model = queryModel.Data;
        var action = form.GetFormValue<string>("action");
        var fileName = form.GetFormValue<string>("backupFileName");
        fileName = _fileProvider.GetFileName(_fileProvider.GetAbsolutePath(fileName));

        var backupPath = _maintenanceService.GetBackupPath(fileName);

        try
        {
            switch (action)
            {
                case "delete-backup":
                    {
                        _fileProvider.DeleteFile(backupPath);
                        return Ok(string.Format(await _localizationService.GetResourceAsync("Admin.System.Maintenance.BackupDatabase.BackupDeleted"), fileName));
                    }
                case "restore-backup":
                    {
                        await _dataProvider.RestoreDatabaseAsync(backupPath);
                        return Ok(await _localizationService.GetResourceAsync("Admin.System.Maintenance.BackupDatabase.DatabaseRestored"));
                    }
                default:
                    return Ok();
            }
        }
        catch (Exception exc)
        {
            //prepare model
            model = await _commonModelFactory.PrepareMaintenanceModelAsync(model);

            return BadRequestWrap(model, null, new List<string> { exc.Message });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> MaintenanceDeleteAlreadySentQueuedEmails([FromBody] BaseQueryModel<MaintenanceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var startDateValue = model.DeleteAlreadySentQueuedEmails.StartDate == null ? null
                        : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteAlreadySentQueuedEmails.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

        var endDateValue = model.DeleteAlreadySentQueuedEmails.EndDate == null ? null
                        : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteAlreadySentQueuedEmails.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        model.DeleteAlreadySentQueuedEmails.NumberOfDeletedEmails = await _queuedEmailService.DeleteAlreadySentEmailsAsync(startDateValue, endDateValue);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> GetLanguage()
    {
        //prepare model
        var model = await _commonModelFactory.PrepareLanguageSelectorModelAsync();

        return OkWrap(model);
    }

    [HttpGet("{langid}")]
    public virtual async Task<IActionResult> SetLanguage(int langid)
    {
        var language = await _languageService.GetLanguageByIdAsync(langid);
        if (language != null)
            await _workContext.SetWorkingLanguageAsync(language);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ClearCache()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        await _staticCacheManager.ClearAsync();

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RestartApplication()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance) &&
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins) &&
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AdminApiAccessDenied();
        }

        //restart application
        _webHelper.RestartAppDomain();

        return Ok();
    }

    public virtual async Task<IActionResult> SeNames()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _commonModelFactory.PrepareUrlRecordSearchModelAsync(new UrlRecordSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SeNames([FromBody] BaseQueryModel<UrlRecordSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _commonModelFactory.PrepareUrlRecordListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelectedSeNames([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return NotFound();

        await _urlRecordService.DeleteUrlRecordsAsync(await _urlRecordService.GetUrlRecordsByIdsAsync(selectedIds.ToArray()));

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PopularSearchTermsReport([FromBody] BaseQueryModel<PopularSearchTermSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _commonModelFactory.PreparePopularSearchTermListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpGet("{entityId}")]
    //action displaying notification (warning) to a store owner that entered SE URL already exists
    public virtual async Task<IActionResult> UrlReservedWarning(string entityId, [FromBody] BaseQueryModel<string> queryModel)
    {
        var form = queryModel.FormValues.ToNameValueCollection();
        var entityName = form.GetFormValue<string>("entityName");
        var seName = form.GetFormValue<string>("seName");

        if (string.IsNullOrEmpty(seName))
            return Ok();

        _ = int.TryParse(entityId, out var parsedEntityId);
        var validatedSeName = await _urlRecordService.ValidateSeNameAsync(parsedEntityId, entityName, seName, null, false);

        if (seName.Equals(validatedSeName, StringComparison.InvariantCultureIgnoreCase))
            return Ok(defaultMessage: true);

        return Ok(string.Format(await _localizationService.GetResourceAsync("Admin.System.Warnings.URL.Reserved"), validatedSeName));
    }

    #endregion
}