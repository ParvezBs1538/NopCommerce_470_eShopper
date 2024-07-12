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
using NopStation.Plugin.AddressValidator.Google.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.AddressValidator.Google.Controllers
{
    public class GoogleAddressValidatorController : NopStationAdminController
    {
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;

        public GoogleAddressValidatorController(IStoreContext storeContext,
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
            if (!await _permissionService.AuthorizeAsync(GoogleAddressValidatorPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var addressValidatorSettings = await _settingService.LoadSettingAsync<GoogleAddressValidatorSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                EnablePlugin = addressValidatorSettings.EnablePlugin,
                GoogleApiKey = addressValidatorSettings.GoogleApiKey,
                EnableLog = addressValidatorSettings.EnableLog,
                PhoneNumberRegex = addressValidatorSettings.PhoneNumberRegex,
                ValidatePhoneNumber = addressValidatorSettings.ValidatePhoneNumber,
                PlotNumberAttributeId = addressValidatorSettings.PlotNumberAttributeId,
                StreetNameAttributeId = addressValidatorSettings.StreetNameAttributeId,
                StreetNumberAttributeId = addressValidatorSettings.StreetNumberAttributeId
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
                model.GoogleApiKey_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.GoogleApiKey, storeScope);
                model.ValidatePhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.ValidatePhoneNumber, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.PhoneNumberRegex, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.EnableLog, storeScope);
                model.PlotNumberAttributeId_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.PlotNumberAttributeId, storeScope);
                model.StreetNameAttributeId_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.StreetNameAttributeId, storeScope);
                model.StreetNumberAttributeId_OverrideForStore = await _settingService.SettingExistsAsync(addressValidatorSettings, x => x.StreetNumberAttributeId, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.AddressValidator.Google/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(GoogleAddressValidatorPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var addressValidatorSettings = await _settingService.LoadSettingAsync<GoogleAddressValidatorSettings>(storeScope);

            addressValidatorSettings.EnablePlugin = model.EnablePlugin;
            addressValidatorSettings.GoogleApiKey = model.GoogleApiKey;
            addressValidatorSettings.ValidatePhoneNumber = model.ValidatePhoneNumber;
            addressValidatorSettings.PhoneNumberRegex = model.PhoneNumberRegex;
            addressValidatorSettings.PlotNumberAttributeId = model.PlotNumberAttributeId;
            addressValidatorSettings.StreetNameAttributeId = model.StreetNameAttributeId;
            addressValidatorSettings.StreetNumberAttributeId = model.StreetNumberAttributeId;
            addressValidatorSettings.EnableLog = model.EnableLog;

            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.GoogleApiKey, model.GoogleApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.PhoneNumberRegex, model.PhoneNumberRegex_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.ValidatePhoneNumber, model.ValidatePhoneNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.PlotNumberAttributeId, model.PlotNumberAttributeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.StreetNameAttributeId, model.StreetNameAttributeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(addressValidatorSettings, x => x.StreetNumberAttributeId, model.StreetNumberAttributeId_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
