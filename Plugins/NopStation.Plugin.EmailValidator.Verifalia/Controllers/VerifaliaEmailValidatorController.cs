using NopStation.Plugin.EmailValidator.Verifalia.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Misc.Core.Filters;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Controllers;
using System.Collections.Generic;
using System;
using NopStation.Plugin.EmailValidator.Verifalia.Services;
using NopStation.Plugin.EmailValidator.Verifalia.Factories;
using Nop.Web.Framework.Mvc;

namespace NopStation.Plugin.EmailValidator.Verifalia.Controllers
{
    public class VerifaliaEmailValidatorController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IVerifaliaEmailModelFactory _verifaliaEmailModelFactory;
        private readonly IVerifaliaEmailService _verifaliaEmailService;

        #endregion

        #region Ctor

        public VerifaliaEmailValidatorController(IStoreContext storeContext,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IVerifaliaEmailModelFactory verifaliaEmailModelFactory,
            IVerifaliaEmailService verifaliaEmailService)
        {
            _storeContext = storeContext;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _verifaliaEmailModelFactory = verifaliaEmailModelFactory;
            _verifaliaEmailService = verifaliaEmailService;
        }

        #endregion

        #region Methods

        #region Configuration

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(VerifaliaEmailValidatorPermissionProvider.ManageVerifalia))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var verifaliaEmailValidatorSettings = await _settingService.LoadSettingAsync<VerifaliaEmailValidatorSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                EnablePlugin = verifaliaEmailValidatorSettings.EnablePlugin,
                ValidateCustomerInfoEmail = verifaliaEmailValidatorSettings.ValidateCustomerInfoEmail,
                ValidateCustomerAddressEmail = verifaliaEmailValidatorSettings.ValidateCustomerAddressEmail,
                Username = verifaliaEmailValidatorSettings.Username,
                EnableLog = verifaliaEmailValidatorSettings.EnableLog,
                Password = verifaliaEmailValidatorSettings.Password,
                ValidateQuality = verifaliaEmailValidatorSettings.ValidateQuality,
                QualityLevel = verifaliaEmailValidatorSettings.QualityLevel,
                AllowRiskyEmails = verifaliaEmailValidatorSettings.AllowRiskyEmails,
                BlockedDomains = string.Join(",", verifaliaEmailValidatorSettings.BlockedDomains),
                RevalidateInvalidEmailsAfterHours = verifaliaEmailValidatorSettings.RevalidateInvalidEmailsAfterHours
            };

            var levels = new string[] { "Standard", "High", "Extreme" };
            model.AvailableQualityLevels = levels.Select(x => new SelectListItem
            {
                Text = x,
                Value = x
            }).ToList();

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.EnablePlugin, storeScope);
                model.ValidateCustomerInfoEmail_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.ValidateCustomerInfoEmail, storeScope);
                model.ValidateCustomerAddressEmail_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.ValidateCustomerAddressEmail, storeScope);
                model.Username_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.Username, storeScope);
                model.ValidateQuality_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.ValidateQuality, storeScope);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.Password, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.EnableLog, storeScope);
                model.QualityLevel_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.QualityLevel, storeScope);
                model.AllowRiskyEmails_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.AllowRiskyEmails, storeScope);
                model.BlockedDomains_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.BlockedDomains, storeScope);
                model.RevalidateInvalidEmailsAfterHours_OverrideForStore = await _settingService.SettingExistsAsync(verifaliaEmailValidatorSettings, x => x.RevalidateInvalidEmailsAfterHours, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.EmailValidator.Verifalia/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(VerifaliaEmailValidatorPermissionProvider.ManageVerifalia))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var verifaliaEmailValidatorSettings = await _settingService.LoadSettingAsync<VerifaliaEmailValidatorSettings>(storeScope);

            verifaliaEmailValidatorSettings.EnablePlugin = model.EnablePlugin;
            verifaliaEmailValidatorSettings.ValidateCustomerInfoEmail = model.ValidateCustomerInfoEmail;
            verifaliaEmailValidatorSettings.ValidateCustomerAddressEmail = model.ValidateCustomerAddressEmail;
            verifaliaEmailValidatorSettings.EnablePlugin = model.EnablePlugin;
            verifaliaEmailValidatorSettings.Username = model.Username;
            verifaliaEmailValidatorSettings.ValidateQuality = model.ValidateQuality;
            verifaliaEmailValidatorSettings.Password = model.Password;
            verifaliaEmailValidatorSettings.QualityLevel = model.QualityLevel;
            verifaliaEmailValidatorSettings.EnableLog = model.EnableLog;
            verifaliaEmailValidatorSettings.AllowRiskyEmails = model.AllowRiskyEmails;
            verifaliaEmailValidatorSettings.BlockedDomains = string.IsNullOrEmpty(model.BlockedDomains) ? new List<string>() :
                model.BlockedDomains.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            verifaliaEmailValidatorSettings.RevalidateInvalidEmailsAfterHours = model.RevalidateInvalidEmailsAfterHours;

            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.ValidateCustomerInfoEmail, model.ValidateCustomerInfoEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.ValidateCustomerAddressEmail, model.ValidateCustomerAddressEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.Username, model.Username_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.ValidateQuality, model.ValidateQuality_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.QualityLevel, model.QualityLevel_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.AllowRiskyEmails, model.AllowRiskyEmails_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.BlockedDomains, model.BlockedDomains_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(verifaliaEmailValidatorSettings, x => x.RevalidateInvalidEmailsAfterHours, model.RevalidateInvalidEmailsAfterHours_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region Verifalia        

        public virtual async Task<IActionResult> EmailList()
        {
            if (!await _permissionService.AuthorizeAsync(VerifaliaEmailValidatorPermissionProvider.ManageVerifalia))
                return AccessDeniedView();

            var searchModel = await _verifaliaEmailModelFactory.PrepareVerifaliaEmailSearchModelAsync(new VerifaliaEmailSearchModel());
            return View("~/Plugins/NopStation.Plugin.EmailValidator.Verifalia/Views/EmailList.cshtml", searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> EmailList(VerifaliaEmailSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(VerifaliaEmailValidatorPermissionProvider.ManageVerifalia))
                return await AccessDeniedDataTablesJson();

            var model = await _verifaliaEmailModelFactory.PrepareVerifaliaEmailListModelAsync(searchModel);
            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> DeleteEmail(int id)
        {
            if (!await _permissionService.AuthorizeAsync(VerifaliaEmailValidatorPermissionProvider.ManageVerifalia))
                return await AccessDeniedDataTablesJson();

            var verifaliaEmail = await _verifaliaEmailService.GetVerifaliaEmailByIdAsync(id) ??
                throw new ArgumentNullException("No specific email found this id");

            await _verifaliaEmailService.DeleteVerifaliaEmailAsync(verifaliaEmail);

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            await _verifaliaEmailService.DeleteVerifaliaEmailsAsync((await _verifaliaEmailService.GetVerifaliaEmailsByIdsAsync(selectedIds.ToArray())).ToList());

            return Json(new { Result = true });
        }

        #endregion

        #endregion
    }
}
