using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Controllers;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Popups.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Popups.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Popups.Domains;
using NopStation.Plugin.Widgets.Popups.Services;

namespace NopStation.Plugin.Widgets.Popups.Areas.Admin.Controller;

public class PopupController : BaseWidgetAdminController
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IWorkContext _workContext;
    private readonly IPluginService _pluginService;
    private readonly IPopupModelFactory _popupModelFactory;
    private readonly IPictureService _pictureService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly IPopupService _popupService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IConditionModelFactory _conditionModelFactory;
    private readonly IAclService _aclService;
    private readonly ICustomerService _customerService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ILanguageService _languageService;

    #endregion

    #region Ctor

    public PopupController(IStoreContext storeContext,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IWorkContext workContext,
        IPluginService pluginService,
        IPopupModelFactory newsletterPopupModelFactory,
        IPictureService pictureService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IPopupService newsletterPopupService,
        ILocalizedEntityService localizedEntityService,
        IConditionModelFactory conditionModelFactory,
        IAclService aclService,
        ICustomerService customerService,
        IStaticCacheManager staticCacheManager,
        ILanguageService languageService)
    {
        _storeContext = storeContext;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _workContext = workContext;
        _pluginService = pluginService;
        _popupModelFactory = newsletterPopupModelFactory;
        _pictureService = pictureService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _popupService = newsletterPopupService;
        _localizedEntityService = localizedEntityService;
        _conditionModelFactory = conditionModelFactory;
        _aclService = aclService;
        _customerService = customerService;
        _staticCacheManager = staticCacheManager;
        _languageService = languageService;
    }

    #endregion

    #region Utilities

    protected virtual async Task SaveStoreMappingsAsync(Popup popup, PopupModel model)
    {
        popup.LimitedToStores = model.SelectedStoreIds.Any();

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(popup);
        var allStores = await _storeService.GetAllStoresAsync();

        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                    await _storeMappingService.InsertStoreMappingAsync(popup, store.Id);
            }
            else
            {
                //remove store
                var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                if (storeMappingToDelete != null)
                    await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
            }
        }
    }

    protected virtual async Task SavePopupAclAsync(Popup popup, PopupModel model)
    {
        popup.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _popupService.UpdatePopupAsync(popup);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(popup);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                    await _aclService.InsertAclRecordAsync(popup, customerRole.Id);
            }
            else
            {
                //remove role
                var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                if (aclRecordToDelete != null)
                    await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
            }
        }
    }

    protected virtual async Task UpdateLocalesAsync(Popup popup, PopupModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(popup,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(popup,
                    x => x.Column1Text,
                    localized.Column1Text,
                    localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(popup,
                    x => x.Column1DesktopPictureId,
                    localized.Column1DesktopPictureId,
                    localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(popup,
                    x => x.Column1PopupUrl,
                    localized.Column1PopupUrl,
                    localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(popup,
                    x => x.Column2Text,
                    localized.Column2Text,
                    localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(popup,
                    x => x.Column2DesktopPictureId,
                    localized.Column2DesktopPictureId,
                    localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(popup,
                    x => x.Column2PopupUrl,
                    localized.Column2PopupUrl,
                    localized.LanguageId);
        }
    }

    #endregion

    #region Methods

    #region Configuration

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var popupSettings = await _settingService.LoadSettingAsync<PopupSettings>(storeId);

        var model = popupSettings.ToSettingsModel<ConfigurationModel>();

        model.ActiveStoreScopeConfiguration = storeId;

        //locales
        await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
        {
            locale.NewsletterPopupTitle = await _localizationService
                .GetLocalizedSettingAsync(popupSettings, x => x.NewsletterPopupTitle, languageId, 0, false, false);
            locale.NewsletterPopupDesctiption = await _localizationService
                .GetLocalizedSettingAsync(popupSettings, x => x.NewsletterPopupDesctiption, languageId, 0, false, false);
        });

        if (storeId > 0)
        {
            model.EnableNewsletterPopup_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.EnableNewsletterPopup, storeId);
            model.NewsletterPopupTitle_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.NewsletterPopupTitle, storeId);
            model.NewsletterPopupDesctiption_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.NewsletterPopupDesctiption, storeId);
            model.BackgroundPictureId_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.BackgroundPictureId, storeId);
            model.RedirectUrl_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.RedirectUrl, storeId);
            model.PopupOpenerSelector_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.PopupOpenerSelector, storeId);
            model.OpenPopupOnLoadPage_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.OpenPopupOnLoadPage, storeId);
            model.DelayTime_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.DelayTime, storeId);
            model.AllowCustomerToSelectDoNotShowThisPopupAgain_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.AllowCustomerToSelectDoNotShowThisPopupAgain, storeId);
            model.PreSelectedDoNotShowThisPopupAgain_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.PreSelectedDoNotShowThisPopupAgain, storeId);
            model.ShowOnHomePageOnly_OverrideForStore = await _settingService.SettingExistsAsync(popupSettings, x => x.ShowOnHomePageOnly, storeId);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var popupSettings = await _settingService.LoadSettingAsync<PopupSettings>(storeScope);

        popupSettings = model.ToSettings(popupSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.EnableNewsletterPopup, model.EnableNewsletterPopup_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.NewsletterPopupTitle, model.NewsletterPopupTitle_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.NewsletterPopupDesctiption, model.NewsletterPopupDesctiption_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.BackgroundPictureId, model.BackgroundPictureId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.RedirectUrl, model.RedirectUrl_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.PopupOpenerSelector, model.PopupOpenerSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.OpenPopupOnLoadPage, model.OpenPopupOnLoadPage_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.DelayTime, model.DelayTime_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.AllowCustomerToSelectDoNotShowThisPopupAgain, model.AllowCustomerToSelectDoNotShowThisPopupAgain_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.PreSelectedDoNotShowThisPopupAgain, model.PreSelectedDoNotShowThisPopupAgain_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(popupSettings, x => x.ShowOnHomePageOnly, model.ShowOnHomePageOnly_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();
        await _staticCacheManager.RemoveByPrefixAsync(PopupCacheDefaults.DefaultPopupModelPrefix);

        //localization. no multi-store support for localization yet.
        foreach (var localized in model.Locales)
        {
            await _localizationService.SaveLocalizedSettingAsync(popupSettings,
                x => x.NewsletterPopupTitle, localized.LanguageId, localized.NewsletterPopupTitle);
            await _localizationService.SaveLocalizedSettingAsync(popupSettings,
                x => x.NewsletterPopupDesctiption, localized.LanguageId, localized.NewsletterPopupDesctiption);
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion

    #region List

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        var model = await _popupModelFactory.PreparePopupSearchModelAsync(new PopupSearchModel());

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> List(PopupSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return await AccessDeniedDataTablesJson();

        var newsletterPopup = await _popupModelFactory.PreparePopupListModelAsync(searchModel);
        return Json(newsletterPopup);
    }

    #endregion

    #region Create / update / delete

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        var model = await _popupModelFactory.PreparePopupModelAsync(new PopupModel(), null);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Create(PopupModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        if (!model.OpenPopupOnLoadPage)
            model.EnableStickyButton = true;

        if (ModelState.IsValid)
        {
            var popup = model.ToEntity<Popup>();
            popup.CreatedOnUtc = DateTime.UtcNow;

            await _popupService.InsertPopupAsync(popup);

            await UpdateLocalesAsync(popup, model);

            await SaveStoreMappingsAsync(popup, model);

            //ACL (customer roles)
            await SavePopupAclAsync(popup, model);

            await _popupService.UpdatePopupAsync(popup);

            //update schedule mappings
            await UpdateScheduleMappingsAsync(model.Schedule, popup, async (popup) => await _popupService.UpdatePopupAsync(popup));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Popups.Popups.Created"));

            return continueEditing
                ? RedirectToAction("Edit", new { id = popup.Id })
                : RedirectToAction("List");
        }

        model = await _popupModelFactory.PreparePopupModelAsync(model, null);

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        var popup = await _popupService.GetPopupByIdAsync(id);
        if (popup == null || popup.Deleted)
            return RedirectToAction("List");

        var model = await _popupModelFactory.PreparePopupModelAsync(null, popup);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(PopupModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        var popup = await _popupService.GetPopupByIdAsync(model.Id);
        if (popup == null || popup.Deleted)
            return RedirectToAction("List");

        if (!model.OpenPopupOnLoadPage)
            model.EnableStickyButton = true;

        if (ModelState.IsValid)
        {
            popup = model.ToEntity(popup);
            await _popupService.UpdatePopupAsync(popup);

            await UpdateLocalesAsync(popup, model);

            await SaveStoreMappingsAsync(popup, model);

            //ACL (customer roles)
            await SavePopupAclAsync(popup, model);

            await _popupService.UpdatePopupAsync(popup);

            //update schedule mappings
            await UpdateScheduleMappingsAsync(model.Schedule, popup, async (popup) => await _popupService.UpdatePopupAsync(popup));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Popups.Popups.Updated"));

            return continueEditing
                ? RedirectToAction("Edit", new { id = model.Id })
                : RedirectToAction("List");
        }

        model = await _popupModelFactory.PreparePopupModelAsync(model, popup);
        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        var popup = await _popupService.GetPopupByIdAsync(id);
        if (popup == null || popup.Deleted)
            return RedirectToAction("List");

        await _popupService.DeletePopupAsync(popup);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Popups.Popups.Deleted"));

        return RedirectToAction("List");
    }

    #endregion

    #region Customer condition mappings

    [HttpPost]
    public virtual async Task<IActionResult> CustomerConditionList(CustomerConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return await AccessDeniedDataTablesJson();

        //try to get a popup with the specified id
        var popup = await _popupService.GetPopupByIdAsync(searchModel.EntityId);
        if (popup == null || popup.Deleted)
            throw new ArgumentException("No popup found with the specified id");

        //prepare model
        var model = await base.CustomerConditionListAsync(searchModel, popup);

        return Json(model);
    }

    public virtual async Task<IActionResult> CustomerConditionDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        //try to get a popup with the specified id
        var popup = await _popupService.GetPopupByIdAsync(entityId);
        if (popup == null || popup.Deleted)
            throw new ArgumentException("No popup found with the specified id");

        await base.CustomerConditionDeleteAsync(id, popup);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> CustomerConditionAddPopup(int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        //try to get a popup with the specified id
        var popup = await _popupService.GetPopupByIdAsync(entityId);
        if (popup == null || popup.Deleted)
            throw new ArgumentException("No popup found with the specified id");

        //prepare model
        var model = await _conditionModelFactory.PrepareAddCustomerToConditionSearchModelAsync(new AddCustomerToConditionSearchModel(), popup);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CustomerConditionAddList(AddCustomerToConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _conditionModelFactory.PrepareAddCustomerToConditionListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CustomerConditionAddPopup(AddCustomerToConditionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        //try to get a popup with the specified id
        var popup = await _popupService.GetPopupByIdAsync(model.EntityId);
        if (popup == null || popup.Deleted)
            throw new ArgumentException("No popup found with the specified id");

        await base.CustomerConditionAddAsync(model, popup);

        ViewBag.RefreshPage = true;

        return View(new AddCustomerToConditionSearchModel());
    }

    #endregion

    #region Product condition mappings

    [HttpPost]
    public virtual async Task<IActionResult> ProductConditionList(ProductConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return await AccessDeniedDataTablesJson();

        //try to get a popup with the specified id
        var popup = await _popupService.GetPopupByIdAsync(searchModel.EntityId);
        if (popup == null || popup.Deleted)
            throw new ArgumentException("No popup found with the specified id");

        //prepare model
        var model = await base.ProductConditionListAsync(searchModel, popup);

        return Json(model);
    }

    public virtual async Task<IActionResult> ProductConditionDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        //try to get a popup with the specified id
        var popup = await _popupService.GetPopupByIdAsync(entityId);
        if (popup == null || popup.Deleted)
            throw new ArgumentException("No popup found with the specified id");

        await base.ProductConditionDeleteAsync(id, popup);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ProductConditionAddPopup(int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        //try to get a popup with the specified id
        var popup = await _popupService.GetPopupByIdAsync(entityId);
        if (popup == null || popup.Deleted)
            throw new ArgumentException("No popup found with the specified id");

        //prepare model
        var model = await _conditionModelFactory.PrepareAddProductToConditionSearchModelAsync(new AddProductToConditionSearchModel(), popup);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductConditionAddList(AddProductToConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _conditionModelFactory.PrepareAddProductToConditionListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ProductConditionAddPopup(AddProductToConditionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
            return AccessDeniedView();

        //try to get a popup with the specified id
        var popup = await _popupService.GetPopupByIdAsync(model.EntityId);
        if (popup == null || popup.Deleted)
            throw new ArgumentException("No popup found with the specified id");

        await base.ProductConditionAddAsync(model, popup);

        ViewBag.RefreshPage = true;

        return View(new AddProductToConditionSearchModel());
    }

    #endregion

    #endregion
}
