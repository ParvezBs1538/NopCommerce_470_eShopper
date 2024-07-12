using NopStation.Plugin.EmailValidator.Abstract.Models;
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
using NopStation.Plugin.EmailValidator.Abstract.Services;
using NopStation.Plugin.EmailValidator.Abstract.Factories;
using Nop.Web.Framework.Mvc;

namespace NopStation.Plugin.EmailValidator.Abstract.Controllers
{
    public class AbstractEmailValidatorController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IAbstractEmailModelFactory _abstractEmailModelFactory;
        private readonly IAbstractEmailService _abstractEmailService;

        #endregion

        #region Ctor

        public AbstractEmailValidatorController(IStoreContext storeContext,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IAbstractEmailModelFactory abstractEmailModelFactory,
            IAbstractEmailService abstractEmailService)
        {
            _storeContext = storeContext;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _abstractEmailModelFactory = abstractEmailModelFactory;
            _abstractEmailService = abstractEmailService;
        }

        #endregion

        #region Methods

        #region Configuration

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AbstractEmailValidatorPermissionProvider.ManageAbstract))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abstractEmailValidatorSettings = await _settingService.LoadSettingAsync<AbstractEmailValidatorSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                EnablePlugin = abstractEmailValidatorSettings.EnablePlugin,
                ValidateCustomerInfoEmail = abstractEmailValidatorSettings.ValidateCustomerInfoEmail,
                ValidateCustomerAddressEmail = abstractEmailValidatorSettings.ValidateCustomerAddressEmail,
                ApiKey = abstractEmailValidatorSettings.ApiKey,
                EnableLog = abstractEmailValidatorSettings.EnableLog,
                AllowRiskyEmails = abstractEmailValidatorSettings.AllowRiskyEmails,
                BlockedDomains = string.Join(",", abstractEmailValidatorSettings.BlockedDomains),
                RevalidateInvalidEmailsAfterHours = abstractEmailValidatorSettings.RevalidateInvalidEmailsAfterHours
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(abstractEmailValidatorSettings, x => x.EnablePlugin, storeScope);
                model.ValidateCustomerInfoEmail_OverrideForStore = await _settingService.SettingExistsAsync(abstractEmailValidatorSettings, x => x.ValidateCustomerInfoEmail, storeScope);
                model.ValidateCustomerAddressEmail_OverrideForStore = await _settingService.SettingExistsAsync(abstractEmailValidatorSettings, x => x.ValidateCustomerAddressEmail, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(abstractEmailValidatorSettings, x => x.ApiKey, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(abstractEmailValidatorSettings, x => x.EnableLog, storeScope);
                model.AllowRiskyEmails_OverrideForStore = await _settingService.SettingExistsAsync(abstractEmailValidatorSettings, x => x.AllowRiskyEmails, storeScope);
                model.BlockedDomains_OverrideForStore = await _settingService.SettingExistsAsync(abstractEmailValidatorSettings, x => x.BlockedDomains, storeScope);
                model.RevalidateInvalidEmailsAfterHours_OverrideForStore = await _settingService.SettingExistsAsync(abstractEmailValidatorSettings, x => x.RevalidateInvalidEmailsAfterHours, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.EmailValidator.Abstract/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AbstractEmailValidatorPermissionProvider.ManageAbstract))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abstractEmailValidatorSettings = await _settingService.LoadSettingAsync<AbstractEmailValidatorSettings>(storeScope);

            abstractEmailValidatorSettings.EnablePlugin = model.EnablePlugin;
            abstractEmailValidatorSettings.ValidateCustomerInfoEmail = model.ValidateCustomerInfoEmail;
            abstractEmailValidatorSettings.ValidateCustomerAddressEmail = model.ValidateCustomerAddressEmail;
            abstractEmailValidatorSettings.EnablePlugin = model.EnablePlugin;
            abstractEmailValidatorSettings.ApiKey = model.ApiKey;
            abstractEmailValidatorSettings.EnableLog = model.EnableLog;
            abstractEmailValidatorSettings.AllowRiskyEmails = model.AllowRiskyEmails;
            abstractEmailValidatorSettings.BlockedDomains = string.IsNullOrEmpty(model.BlockedDomains) ? new List<string>() :
                model.BlockedDomains.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            abstractEmailValidatorSettings.RevalidateInvalidEmailsAfterHours = model.RevalidateInvalidEmailsAfterHours;

            await _settingService.SaveSettingOverridablePerStoreAsync(abstractEmailValidatorSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abstractEmailValidatorSettings, x => x.ValidateCustomerInfoEmail, model.ValidateCustomerInfoEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abstractEmailValidatorSettings, x => x.ValidateCustomerAddressEmail, model.ValidateCustomerAddressEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abstractEmailValidatorSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abstractEmailValidatorSettings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abstractEmailValidatorSettings, x => x.AllowRiskyEmails, model.AllowRiskyEmails_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abstractEmailValidatorSettings, x => x.BlockedDomains, model.BlockedDomains_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abstractEmailValidatorSettings, x => x.RevalidateInvalidEmailsAfterHours, model.RevalidateInvalidEmailsAfterHours_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region Abstract        

        public virtual async Task<IActionResult> EmailList()
        {
            if (!await _permissionService.AuthorizeAsync(AbstractEmailValidatorPermissionProvider.ManageAbstract))
                return AccessDeniedView();

            var searchModel = await _abstractEmailModelFactory.PrepareAbstractEmailSearchModelAsync(new AbstractEmailSearchModel());
            return View("~/Plugins/NopStation.Plugin.EmailValidator.Abstract/Views/EmailList.cshtml", searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> EmailList(AbstractEmailSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AbstractEmailValidatorPermissionProvider.ManageAbstract))
                return await AccessDeniedDataTablesJson();

            var model = await _abstractEmailModelFactory.PrepareAbstractEmailListModelAsync(searchModel);
            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> DeleteEmail(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AbstractEmailValidatorPermissionProvider.ManageAbstract))
                return await AccessDeniedDataTablesJson();

            var abstractEmail = await _abstractEmailService.GetAbstractEmailByIdAsync(id) ??
                throw new ArgumentNullException("No specific email found this id");

            await _abstractEmailService.DeleteAbstractEmailAsync(abstractEmail);

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            await _abstractEmailService.DeleteAbstractEmailsAsync((await _abstractEmailService.GetAbstractEmailsByIdsAsync(selectedIds.ToArray())).ToList());

            return Json(new { Result = true });
        }

        #endregion

        #endregion
    }
}
