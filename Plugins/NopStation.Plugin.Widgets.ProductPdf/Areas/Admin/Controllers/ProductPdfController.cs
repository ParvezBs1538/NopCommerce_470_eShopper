using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ProductPdf.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.ProductPdf.Areas.Admin.Controllers
{
    public class ProductPdfController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public ProductPdfController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(ProductPdfPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var productPdfSettings = await _settingService.LoadSettingAsync<ProductPdfSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                EnablePlugin = productPdfSettings.EnablePlugin,
                LetterPageSizeEnabled = productPdfSettings.LetterPageSizeEnabled
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(productPdfSettings, x => x.EnablePlugin, storeScope);
                model.LetterPageSizeEnabled_OverrideForStore = await _settingService.SettingExistsAsync(productPdfSettings, x => x.LetterPageSizeEnabled, storeScope);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if(!await _permissionService.AuthorizeAsync(ProductPdfPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var productPdfSettings = await _settingService.LoadSettingAsync<ProductPdfSettings>(storeScope);

            //save settings
            productPdfSettings.EnablePlugin = model.EnablePlugin;
            productPdfSettings.LetterPageSizeEnabled = model.LetterPageSizeEnabled;

            await _settingService.SaveSettingOverridablePerStoreAsync(productPdfSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productPdfSettings, x => x.LetterPageSizeEnabled, model.LetterPageSizeEnabled_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
           
            return RedirectToAction("Configure");
        }

        #endregion
    }
}