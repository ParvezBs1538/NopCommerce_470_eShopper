using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Topics;
using Nop.Web.Areas.Admin.Models.Topics;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ExportImportTopic.Models;
using NopStation.Plugin.Widgets.ExportImportTopic.Services;

namespace NopStation.Plugin.Widgets.ExportImportTopic.Controller
{
    public partial class ExportImportTopicController : NopStationAdminController
    {
        #region Fields

        private readonly IExImManager _exImManager;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ITopicService _topicService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ExportImportTopicSettings _exportImportTopicSettings;

        #endregion

        #region Ctor

        public ExportImportTopicController(IExImManager exImManager,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ITopicService topicService,
            ISettingService settingService,
            IStoreContext storeContext,
            ExportImportTopicSettings exportImportTopicSettings)
        {
            _exImManager = exImManager;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _topicService = topicService;
            _settingService = settingService;
            _storeContext = storeContext;
            _exportImportTopicSettings = exportImportTopicSettings;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(ExportImportTopicPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var exportImportTopicSettings = await _settingService.LoadSettingAsync<ExportImportTopicSettings>(storeId);

            var model = new ConfigurationModel()
            {
                CheckBodyMaximumLength = exportImportTopicSettings.CheckBodyMaximumLength,
                BodyMaximumLength = exportImportTopicSettings.BodyMaximumLength
            };

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId > 0)
            {
                model.CheckBodyMaximumLength_OverrideForStore = await _settingService.SettingExistsAsync(exportImportTopicSettings, x => x.CheckBodyMaximumLength, storeId);
                model.BodyMaximumLength_OverrideForStore = await _settingService.SettingExistsAsync(exportImportTopicSettings, x => x.BodyMaximumLength, storeId);
            }

            return View("~/Plugins/NopStation.Plugin.Widgets.ExportImportTopic/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ExportImportTopicPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var exportImportTopicSettings = await _settingService.LoadSettingAsync<ExportImportTopicSettings>(storeScope);

            exportImportTopicSettings.CheckBodyMaximumLength = model.CheckBodyMaximumLength;
            exportImportTopicSettings.BodyMaximumLength = model.BodyMaximumLength;

            await _settingService.SaveSettingOverridablePerStoreAsync(exportImportTopicSettings, x => x.CheckBodyMaximumLength, model.CheckBodyMaximumLength_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(exportImportTopicSettings, x => x.BodyMaximumLength, model.BodyMaximumLength_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [HttpPost]
        public virtual async Task<IActionResult> Export(TopicSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            //get topics
            var topics = (await _topicService.GetAllTopicsAsync(showHidden: true,
                keywords: searchModel.SearchKeywords,
                storeId: searchModel.SearchStoreId,
                ignoreAcl: true))
                .Where(t => string.IsNullOrWhiteSpace(t.Body) || !_exportImportTopicSettings.CheckBodyMaximumLength || t.Body.Length <= _exportImportTopicSettings.BodyMaximumLength)
                .ToList();

            try
            {
                var bytes = await _exImManager.ExportToXlsxAsync(topics);

                return File(bytes, MimeTypes.TextXlsx, "topics.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("List", "Topic");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> Import(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                    await _exImManager.ImportFromXlsxAsync(importexcelfile.OpenReadStream());
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

                    return RedirectToAction("List", "Topic");
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ExportImportTopic.Imported"));

                return RedirectToAction("List", "Topic");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("List", "Topic");
            }
        }

        #endregion
    }
}