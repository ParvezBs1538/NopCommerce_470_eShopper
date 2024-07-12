using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nopstation.Plugin.Widgets.DooFinder.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace Nopstation.Plugin.Widgets.DooFinder.Controllers
{
    public class FeedDooFinderController : NopStationAdminController
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _nopFileProvider;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;
        private readonly IPermissionService _permissionService;
        private readonly IPluginService _pluginService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public FeedDooFinderController(ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INopFileProvider nopFileProvider,
            INotificationService notificationService,
            ILogger logger,
            IPermissionService permissionService,
            IPluginService pluginService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWebHelper webHelper,
            IWebHostEnvironment webHostEnvironment,
            IWorkContext workContext)
        {
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _nopFileProvider = nopFileProvider;
            _notificationService = notificationService;
            _logger = logger;
            _permissionService = permissionService;
            _pluginService = pluginService;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _webHelper = webHelper;
            _webHostEnvironment = webHostEnvironment;
            _workContext = workContext;
        }

        #endregion

        #region Utilites

        private async Task PrepareModelAsync(FeedDooFinderModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dooFinderSettings = await _settingService.LoadSettingAsync<DooFinderSettings>(storeScope);

            model.ProductPictureSize = dooFinderSettings.ProductPictureSize;
            model.PricesConsiderPromotions = dooFinderSettings.PricesConsiderPromotions;
            model.ApiScript = dooFinderSettings.ApiScript;
            model.ActiveScript = dooFinderSettings.ActiveScript;
            model.AddAttributes = dooFinderSettings.AddAttributes;

            //currencies
            model.CurrencyId = dooFinderSettings.CurrencyId;
            foreach (var c in await _currencyService.GetAllCurrenciesAsync())
                model.AvailableCurrencies.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            model.HideConfigBlock = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, DooFinderDefaults.HideConfigBlock);
            model.HideSchedulerBlock = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, DooFinderDefaults.HideSchedulerBlock);
            model.HideSettingsBlock = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, DooFinderDefaults.HideSettingsBlock);

            // Schedule Feed Generating Time
            model.ScheduleFeedGeneratingHour = dooFinderSettings.ScheduleFeedGeneratingHour;
            model.ScheduleFeedGeneratingMinute = dooFinderSettings.ScheduleFeedGeneratingMinute;
            model.ScheduleFeedLastExecutionStartTime = dooFinderSettings.ScheduleFeedLastExecutionStartTime;
            model.ScheduleFeedLastExecutionEndTime = dooFinderSettings.ScheduleFeedLastExecutionEndTime;
            model.IsFeedGenerating = dooFinderSettings.IsFeedGenerating;

            var nextExecutionTime = DateTime.UtcNow.Date
                .AddHours(dooFinderSettings.ScheduleFeedGeneratingHour)
                .AddMinutes(dooFinderSettings.ScheduleFeedGeneratingMinute);

            if (dooFinderSettings.ScheduleFeedIsExecutedForToday)
            {
                nextExecutionTime = nextExecutionTime.AddDays(1);
            }
            model.ScheduleFeedNextExecutionTime = nextExecutionTime;

            //file paths
            foreach (var store in await _storeService.GetAllStoresAsync())
            {
                var localFilePath = _nopFileProvider.Combine(_webHostEnvironment.WebRootPath, "files", "exportimport", store.Id + "-" + dooFinderSettings.StaticFileName);
                if (_nopFileProvider.FileExists(localFilePath))
                    model.GeneratedFiles.Add(new GeneratedFileModel
                    {
                        StoreName = store.Name,
                        FileUrl = $"{_webHelper.GetStoreLocation(false)}files/exportimport/{store.Id}-{dooFinderSettings.StaticFileName}"
                    });
            }

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.CurrencyId_OverrideForStore = await _settingService.SettingExistsAsync(dooFinderSettings, x => x.CurrencyId, storeScope);
                model.PricesConsiderPromotions_OverrideForStore = await _settingService.SettingExistsAsync(dooFinderSettings, x => x.PricesConsiderPromotions, storeScope);
                model.ProductPictureSize_OverrideForStore = await _settingService.SettingExistsAsync(dooFinderSettings, x => x.ProductPictureSize, storeScope);
                model.ScheduleFeedGeneratingHour_OverrideForStore = await _settingService.SettingExistsAsync(dooFinderSettings, x => x.ScheduleFeedGeneratingHour, storeScope);
                model.ScheduleFeedGeneratingMinute_OverrideForStore = await _settingService.SettingExistsAsync(dooFinderSettings, x => x.ScheduleFeedGeneratingMinute, storeScope);
                model.ApiScript_OverrideForStore = await _settingService.SettingExistsAsync(dooFinderSettings, x => x.ApiScript, storeScope);
                model.ActiveScript_OverrideForStore = await _settingService.SettingExistsAsync(dooFinderSettings, x => x.ActiveScript, storeScope);
                model.AddAttributes_OverrideForStore = await _settingService.SettingExistsAsync(dooFinderSettings, x => x.AddAttributes, storeScope);
            }

            for (var i = 0; i < 60; i++)
            {
                var minute = new SelectListItem { Text = i.ToString("00"), Value = i.ToString() };
                model.AvailableMinutes.Add(minute);
            }

            for (var i = 0; i < 24; i++)
            {
                var hour = new SelectListItem { Text = i.ToString("00"), Value = i.ToString() };
                model.AvailableHours.Add(hour);
            }
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(DooFinderPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = new FeedDooFinderModel();
            await PrepareModelAsync(model);

            return View("~/Plugins/Nopstation.Plugin.Widgets.DooFinder/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(FeedDooFinderModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DooFinderPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return await Configure();
            }

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dooFinderSettings = await _settingService.LoadSettingAsync<DooFinderSettings>(storeScope);

            //save settings
            dooFinderSettings.ProductPictureSize = model.ProductPictureSize;
            dooFinderSettings.PricesConsiderPromotions = model.PricesConsiderPromotions;
            dooFinderSettings.CurrencyId = model.CurrencyId;
            dooFinderSettings.ScheduleFeedGeneratingHour = model.ScheduleFeedGeneratingHour;
            dooFinderSettings.ScheduleFeedGeneratingMinute = model.ScheduleFeedGeneratingMinute;
            dooFinderSettings.ApiScript = model.ApiScript;
            dooFinderSettings.AddAttributes = model.AddAttributes;
            dooFinderSettings.ActiveScript = model.ActiveScript;

            _settingService.SaveSetting(dooFinderSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(dooFinderSettings, x => x.CurrencyId, model.CurrencyId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dooFinderSettings, x => x.PricesConsiderPromotions, model.PricesConsiderPromotions_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dooFinderSettings, x => x.ProductPictureSize, model.ProductPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dooFinderSettings, x => x.ScheduleFeedGeneratingHour, model.ScheduleFeedGeneratingHour_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dooFinderSettings, x => x.ScheduleFeedGeneratingMinute, model.ScheduleFeedGeneratingMinute_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dooFinderSettings, x => x.ApiScript, model.ApiScript_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dooFinderSettings, x => x.AddAttributes, model.AddAttributes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dooFinderSettings, x => x.ActiveScript, model.ActiveScript_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            //redisplay the form
            return await Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("generate")]
        public async Task<IActionResult> GenerateFeed(FeedDooFinderModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DooFinderPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            try
            {
                //plugin
                var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(DooFinderDefaults.PLUGIN_SYSTEM_NAME);
                if (pluginDescriptor == null || pluginDescriptor.Instance<IPlugin>() is not DooFinderPlugin plugin)
                    throw new Exception(await _localizationService.GetResourceAsync("Nopstation.Plugin.Widgets.DooFinder.ExceptionLoadPlugin"));

                var stores = new List<Store>();
                var storeById = await _storeService.GetStoreByIdAsync(storeScope);
                if (storeScope > 0)
                    stores.Add(storeById);
                else
                    stores.AddRange(await _storeService.GetAllStoresAsync());

                foreach (var store in stores)
                    await plugin.GenerateStaticFileAsync(store);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Nopstation.Plugin.Widgets.DooFinder.SuccessResult"));
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                await _logger.ErrorAsync(exc.Message, exc);
            }

            return await Configure();
        }

        #endregion
    }
}