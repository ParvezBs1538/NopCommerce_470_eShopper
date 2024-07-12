using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Localization;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Cache;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Controllers
{
    public class WebApiController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ILanguageService _languageService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWebApiModelFactory _webApiModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IApiStringResourceService _apiStringResourceService;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public WebApiController(ILocalizationService localizationService,
            INotificationService notificationService,
            ILanguageService languageService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWebApiModelFactory webApiModelFactory,
            IPermissionService permissionService,
            IApiStringResourceService apiStringResourceService,
            IStaticCacheManager cacheManager)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _languageService = languageService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _webApiModelFactory = webApiModelFactory;
            _permissionService = permissionService;
            _apiStringResourceService = apiStringResourceService;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        #region Configuration

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _webApiModelFactory.PrepareConfigurationModelAsync();
            return View("~/Plugins/NopStation.Plugin.Misc.WebApi/Areas/Admin/Views/WebApi/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var webApiSettings = await _settingService.LoadSettingAsync<WebApiSettings>(storeScope);
            webApiSettings = model.ToSettings(webApiSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.CheckIat, model.CheckIat_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.IOSProductPriceTextSize, model.IOSProductPriceTextSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.IonicProductPriceTextSize, model.IonicProductPriceTextSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.AndroidProductPriceTextSize, model.AndroidProductPriceTextSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.ShowHomepageSlider, model.ShowHomepageSlider_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.SliderAutoPlay, model.SliderAutoPlay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.SliderAutoPlayTimeout, model.SliderAutoPlayTimeout_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.EnableJwtSecurity, model.EnableJwtSecurity_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.MaximumNumberOfHomePageSliders, model.MaximumNumberOfHomePageSliders_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.NumberOfHomepageCategoryProducts, model.NumberOfHomepageCategoryProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.NumberOfManufacturers, model.NumberOfManufacturers_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.ShowBestsellersOnHomepage, model.ShowBestsellersOnHomepage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.NumberOfBestsellersOnHomepage, model.NumberOfBestsellersOnHomepage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.ShowFeaturedProducts, model.ShowFeaturedProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.ShowHomepageCategoryProducts, model.ShowHomepageCategoryProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.ShowManufacturers, model.ShowManufacturers_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.ShowSubCategoryProducts, model.ShowSubCategoryProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.TokenKey, model.TokenKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.TokenSecondsValid, model.TokenSecondsValid_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.TokenSecret, model.TokenSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.AndroidVersion, model.AndroidVersion_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.AndriodForceUpdate, model.AndriodForceUpdate_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.PlayStoreUrl, model.PlayStoreUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.IOSVersion, model.IOSVersion_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.IOSForceUpdate, model.IOSForceUpdate_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.AppStoreUrl, model.AppStoreUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.LogoId, model.LogoId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.LogoSize, model.LogoSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.ShowChangeBaseUrlPanel, model.ShowChangeBaseUrlPanel_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.PrimaryThemeColor, model.PrimaryThemeColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.BottomBarActiveColor, model.BottomBarActiveColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.BottomBarBackgroundColor, model.BottomBarBackgroundColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.BottomBarInactiveColor, model.BottomBarInactiveColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.TopBarBackgroundColor, model.TopBarBackgroundColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.TopBarTextColor, model.TopBarTextColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.GradientStartingColor, model.GradientStartingColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.GradientMiddleColor, model.GradientMiddleColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.GradientEndingColor, model.GradientEndingColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.GradientEnabled, model.GradientEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.ProductBarcodeScanKeyId, model.ProductBarcodeScanKeyId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.AllowCustomersToDeleteAccount, model.AllowCustomersToDeleteAccount_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SliderPrefixCacheKey);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region Resources

        [HttpPost]
        public virtual async Task<IActionResult> Resources(LocaleResourceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            //try to get a language with the specified id
            var language = await _languageService.GetLanguageByIdAsync(searchModel.LanguageId);
            if (language == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _webApiModelFactory.PrepareLocaleResourceListModelAsync(searchModel, language);

            return Json(model);
        }

        //ValidateAttribute is used to force model validation
        [EditAccessAjax, HttpPost]
        public virtual async Task<ActionResult> ResourceUpdate([Validate] LocaleResourceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var tokens = model.CombinedId.Split("__");
            var apiResourceId = Convert.ToInt32(tokens[0]);
            var resourceId = Convert.ToInt32(tokens[1]);
            var languageId = Convert.ToInt32(tokens[2]);

            if (model.ResourceName != null)
                model.ResourceName = model.ResourceName.Trim();
            if (model.ResourceValue != null)
                model.ResourceValue = model.ResourceValue.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var apiResource = await _apiStringResourceService.GetApiStringResourceByIdAsync(apiResourceId);
            if (!apiResource.ResourceName.Equals(model.ResourceName, StringComparison.InvariantCultureIgnoreCase))
            {
                var apiRes = await _apiStringResourceService.GetApiStringResourceByNameAsync(model.ResourceName);
                if (apiRes != null && apiRes.Id != apiResource.Id)
                    return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Resources.NameAlreadyExists"), model.ResourceName));
            }

            var resource = await _localizationService.GetLocaleStringResourceByIdAsync(resourceId);
            if (resource != null && !resource.ResourceName.Equals(model.ResourceName, StringComparison.InvariantCultureIgnoreCase))
            {
                var res = await _localizationService.GetLocaleStringResourceByNameAsync(model.ResourceName, languageId, false);
                if (res != null && res.Id != resource.Id)
                    return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Languages.Resources.NameAlreadyExists"), res.ResourceName));
            }

            resource = resource ?? await _localizationService.GetLocaleStringResourceByNameAsync(model.ResourceName, languageId, false);
            if (resource == null)
            {
                resource = new LocaleStringResource()
                {
                    LanguageId = languageId,
                    ResourceName = model.ResourceName,
                    ResourceValue = model.ResourceValue
                };
                await _localizationService.InsertLocaleStringResourceAsync(resource);
            }
            else
            {
                resource.ResourceName = model.ResourceName;
                resource.ResourceValue = model.ResourceValue;
                await _localizationService.UpdateLocaleStringResourceAsync(resource);
            }

            apiResource.ResourceName = model.ResourceName;
            await _apiStringResourceService.UpdateApiStringResourceAsync(apiResource);

            return new NullJsonResult();
        }

        //ValidateAttribute is used to force model validation
        [EditAccessAjax, HttpPost]
        public virtual async Task<ActionResult> ResourceAdd(int languageId, [Validate] LocaleResourceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            if (model.ResourceName != null)
                model.ResourceName = model.ResourceName.Trim();
            if (model.ResourceValue != null)
                model.ResourceValue = model.ResourceValue.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var res = await _localizationService.GetLocaleStringResourceByNameAsync(model.ResourceName, model.LanguageId, false);
            if (res == null)
            {
                //fill entity from model
                var resource = model.ToEntity<LocaleStringResource>();

                resource.LanguageId = languageId;

                await _localizationService.InsertLocaleStringResourceAsync(resource);

                var ar = await _apiStringResourceService.GetApiStringResourceByNameAsync(resource.ResourceName);
                if (ar == null)
                {
                    var appResource = new ApiStringResource()
                    {
                        ResourceName = resource.ResourceName
                    };
                    await _apiStringResourceService.InsertApiStringResourceAsync(appResource);
                }
            }
            else
            {
                var ar = await _apiStringResourceService.GetApiStringResourceByNameAsync(res.ResourceName);
                if (ar == null)
                    return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Resources.NameAlreadyExists"),
                        model.ResourceName, await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Resources.AddFromExistingRecords")));
                else
                    return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Languages.Resources.NameAlreadyExists"), res.ResourceName));
            }

            return Json(new { Result = true });
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<ActionResult> ResourceDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var appResource = await _apiStringResourceService.GetApiStringResourceByIdAsync(id);
            if (appResource != null)
                await _apiStringResourceService.DeleteApiStringResourceAsync(appResource);

            return new NullJsonResult();
        }

        public async Task<IActionResult> ExistingResourceAddPopup(int languageId)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = new ConfigurationModel();
            model.LocaleResourceSearchModel.LanguageId = languageId;

            return View(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> ExistingResourceAddPopup(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (model.SelectedResourceIds.Any())
            {
                foreach (var item in model.SelectedResourceIds)
                {
                    var resource = await _localizationService.GetLocaleStringResourceByIdAsync(item);
                    if (resource == null)
                        continue;

                    var ar = await _apiStringResourceService.GetApiStringResourceByNameAsync(resource.ResourceName);
                    if (ar == null)
                    {
                        var appResource = new ApiStringResource()
                        {
                            ResourceName = resource.ResourceName
                        };
                        await _apiStringResourceService.InsertApiStringResourceAsync(appResource);
                    }
                }
            }

            ViewBag.RefreshPage = true;

            return View(new ConfigurationModel());
        }

        #endregion

        #endregion
    }
}
