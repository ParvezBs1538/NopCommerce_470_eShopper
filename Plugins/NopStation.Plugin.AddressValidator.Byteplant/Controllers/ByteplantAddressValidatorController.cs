using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Attributes;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.AddressValidator.Byteplant.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.AddressValidator.Byteplant.Controllers
{
    public class ByteplantAddressValidatorController : NopStationAdminController
    {
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;

        public ByteplantAddressValidatorController(IStoreContext storeContext,
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
            if (!await _permissionService.AuthorizeAsync(ByteplantAddressValidatorPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var addressValidatorSettings = await _settingService.LoadSettingAsync<ByteplantAddressValidatorSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                EnablePlugin = addressValidatorSettings.EnablePlugin,
                ByteplantApiKey = addressValidatorSettings.ByteplantApiKey,
                EnableLog = addressValidatorSettings.EnableLog,
                PhoneNumberRegex = addressValidatorSettings.PhoneNumberRegex,
                ValidatePhoneNumber = addressValidatorSettings.ValidatePhoneNumber
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
                model.ByteplantApiKey_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.ByteplantApiKey, storeScope);
                model.ValidatePhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.ValidatePhoneNumber, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.PhoneNumberRegex, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.EnableLog, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.AddressValidator.Byteplant/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ByteplantAddressValidatorPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var addressValidatorSettings = await _settingService.LoadSettingAsync<ByteplantAddressValidatorSettings>(storeScope);

            addressValidatorSettings.EnablePlugin = model.EnablePlugin;
            addressValidatorSettings.ByteplantApiKey = model.ByteplantApiKey;
            addressValidatorSettings.ValidatePhoneNumber = model.ValidatePhoneNumber;
            addressValidatorSettings.PhoneNumberRegex = model.PhoneNumberRegex;
            addressValidatorSettings.EnableLog = model.EnableLog;

            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.ByteplantApiKey, model.ByteplantApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.PhoneNumberRegex, model.PhoneNumberRegex_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.ValidatePhoneNumber, model.ValidatePhoneNumber_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
