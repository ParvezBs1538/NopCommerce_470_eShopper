using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Controllers
{
    public class ProductQAConfigurationController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public ProductQAConfigurationController(IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreService storeService,
            ICustomerService customerService)
        {
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeService = storeService;
            _customerService = customerService;
        }

        #endregion

        #region Utility
        private async Task GetAvailablePropertiesForConfigureMethod(ConfigurationModel model)
        {
            //split limited customer role
            if (!string.IsNullOrEmpty(model.LimitedCustomerRole))
            {
                string[] splitLimitedCustomerRole = model.LimitedCustomerRole.Split(',');
                foreach (var splitedLimitedCustomerRole in splitLimitedCustomerRole)
                {
                    model.SelectedLimitedCustomerRoleIds.Add(Convert.ToInt32(splitedLimitedCustomerRole));
                }
            }
            //split who asnwered customer role
            if (!string.IsNullOrEmpty(model.AnswerdCustomerRole))
            {
                string[] splitAnsweredCustomerRole = model.AnswerdCustomerRole.Split(',');
                foreach (var splitedAnswerCustomerRole in splitAnsweredCustomerRole)
                {
                    model.SelectedAnsweredCustomerRoleIds.Add(Convert.ToInt32(splitedAnswerCustomerRole));
                }
            }

            //split store id
            if (!string.IsNullOrEmpty(model.LimitedStoreId))
            {
                string[] splitStore = model.LimitedStoreId.Split(',');
                foreach (var splitedStore in splitStore)
                {
                    model.SelectedLimitedStoreId.Add(Convert.ToInt32(splitedStore));
                }
            }
            //get all customer role
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync();
            if (allCustomerRoles.Count > 0)
            {
                //prepare limited customer role
                model.AvailableLimitedCustomerRoles = allCustomerRoles.Select(role => new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = model.SelectedLimitedCustomerRoleIds.Contains(role.Id)
                }).ToList();

                //prepare who answered customer role
                model.AvailableAnsweredCustomerRoles = allCustomerRoles.Select(role => new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = model.SelectedAnsweredCustomerRoleIds.Contains(role.Id)
                }).ToList();
            }

            //get all store and check 
            var allStore = await _storeService.GetAllStoresAsync();
            if (allStore.Count > 0)
            {
                //prepare store model
                model.AvailableLimitedStoreId = allStore.Select(role => new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = model.SelectedLimitedStoreId.Contains(role.Id)
                }).ToList();
            }
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(ProductQAPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var productQASettings = await _settingService.LoadSettingAsync<ProductQAConfigurationSettings>(storeScope);
            var model = new ConfigurationModel
            {
                IsEnable = productQASettings.IsEnable,
                QuestionAnonymous = productQASettings.QuestionAnonymous,
                LimitedCustomerRole = productQASettings.LimitedCustomerRole,
                AnswerdCustomerRole = productQASettings.AnswerdCustomerRole,
                LimitedStoreId = productQASettings.LimitedStoreId
            };
            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.IsEnable_OverrideForStore = await _settingService.SettingExistsAsync(productQASettings, x => x.IsEnable, storeScope);
                model.QuestionAnonymous_OverrideForStore = await _settingService.SettingExistsAsync(productQASettings, x => x.QuestionAnonymous, storeScope);
                model.LimitedCustomer_OverrideForStore = await _settingService.SettingExistsAsync(productQASettings, x => x.LimitedCustomerRole, storeScope);
                model.AnswerdCustomerRole_OverrideForStore = await _settingService.SettingExistsAsync(productQASettings, x => x.AnswerdCustomerRole, storeScope);
                model.LimitedStoreId_OverrideForStore = await _settingService.SettingExistsAsync(productQASettings, x => x.LimitedStoreId, storeScope);
            }

            await GetAvailablePropertiesForConfigureMethod(model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ProductQAPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (model.SelectedAnsweredCustomerRoleIds.Count <= 0 || model.SelectedLimitedCustomerRoleIds.Count <= 0)
            {
                await GetAvailablePropertiesForConfigureMethod(model);
                return View(model);
            }
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var productQASettings = await _settingService.LoadSettingAsync<ProductQAConfigurationSettings>(storeScope);

            productQASettings.IsEnable = model.IsEnable;
            productQASettings.QuestionAnonymous = model.QuestionAnonymous;

            if (model.SelectedLimitedStoreId.Count > 0)
            {
                productQASettings.LimitedStoreId = string.Join(',', model.SelectedLimitedStoreId);
            }
            else
            {
                productQASettings.LimitedStoreId = "0";
            }

            if (model.SelectedLimitedCustomerRoleIds.Count > 0)
            {
                productQASettings.LimitedCustomerRole = string.Join(',', model.SelectedLimitedCustomerRoleIds);
            }

            if (model.SelectedAnsweredCustomerRoleIds.Count > 0)
            {
                productQASettings.AnswerdCustomerRole = string.Join(',', model.SelectedAnsweredCustomerRoleIds);
            }

            await _settingService.SaveSettingOverridablePerStoreAsync(productQASettings, x => x.IsEnable, model.IsEnable_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productQASettings, x => x.QuestionAnonymous, model.QuestionAnonymous_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productQASettings, x => x.LimitedCustomerRole, model.LimitedCustomer_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productQASettings, x => x.AnswerdCustomerRole, model.AnswerdCustomerRole_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productQASettings, x => x.LimitedStoreId, model.LimitedStoreId_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.Configuration.Updated"));
            return RedirectToAction("Configure");
        }

        #endregion
    }
}