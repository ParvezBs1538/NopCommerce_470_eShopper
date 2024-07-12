using NopStation.Plugin.AddressValidator.EasyPost.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Misc.Core.Filters;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Controllers;
using Nop.Services.Attributes;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.AddressValidator.EasyPost.Controllers
{
    public class EasyPostAddressValidatorController : NopStationAdminController
    {
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;

        public EasyPostAddressValidatorController(IStoreContext storeContext,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService)
        {
            _storeContext = storeContext;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _addressAttributeService = addressAttributeService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(EasyPostAddressValidatorPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var addressValidatorSettings = await _settingService.LoadSettingAsync<EasyPostAddressValidatorSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                EnablePlugin = addressValidatorSettings.EnablePlugin,
                EasyPostApiKey = addressValidatorSettings.EasyPostApiKey,
                EnableLog = addressValidatorSettings.EnableLog
            };

            var addressAttributes = await _addressAttributeService.GetAllAttributesAsync();
            model.AvailableAddressAttributes = await addressAttributes.SelectAwait(async x => new SelectListItem
            {
                Text = await _localizationService.GetLocalizedAsync(x, y=> y.Name),
                Value = x.Id.ToString()
            }).ToListAsync();

            model.AvailableAddressAttributes.Insert(0, new SelectListItem()
            {
                Value = "0",
                Text = "--"
            });

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.EnablePlugin, storeScope);
                model.EasyPostApiKey_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.EasyPostApiKey, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.EnableLog, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.AddressValidator.EasyPost/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(EasyPostAddressValidatorPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var addressValidatorSettings = await _settingService.LoadSettingAsync<EasyPostAddressValidatorSettings>(storeScope);

            addressValidatorSettings.EnablePlugin = model.EnablePlugin;
            addressValidatorSettings.EasyPostApiKey = model.EasyPostApiKey;
            addressValidatorSettings.EnableLog = model.EnableLog;

            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.EasyPostApiKey, model.EasyPostApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
