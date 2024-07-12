using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
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
using NopStation.Plugin.Widgets.Announcement.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Announcement.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Announcement.Domains;
using NopStation.Plugin.Widgets.Announcement.Services;

namespace NopStation.Plugin.Widgets.Announcement.Areas.Admin.Controllers;

public class AnnouncementController : BaseWidgetAdminController
{
    #region Firlds

    private readonly IPermissionService _permissionService;
    private readonly IAnnouncementItemService _announcementItemService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IAnnouncementItemModelFactory _announcementItemModelFactory;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly IConditionModelFactory _conditionModelFactory;
    private readonly ICustomerService _customerService;
    private readonly IAclService _aclService;

    #endregion

    #region Ctor

    public AnnouncementController(IPermissionService permissionService,
        IAnnouncementItemService announcementItemService,
        ISettingService settingService,
        IStoreContext storeContext,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IAnnouncementItemModelFactory announcementItemModelFactory,
        IDateTimeHelper dateTimeHelper,
        ILocalizedEntityService localizedEntityService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IConditionModelFactory conditionModelFactory,
        ICustomerService customerService,
        IAclService aclService)
    {
        _permissionService = permissionService;
        _announcementItemService = announcementItemService;
        _settingService = settingService;
        _storeContext = storeContext;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _announcementItemModelFactory = announcementItemModelFactory;
        _dateTimeHelper = dateTimeHelper;
        _localizedEntityService = localizedEntityService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _conditionModelFactory = conditionModelFactory;
        _customerService = customerService;
        _aclService = aclService;
    }

    #endregion

    #region Utilities

    protected virtual async Task SaveStoreMappingsAsync(AnnouncementItem announcementItem, AnnouncementItemModel model)
    {
        announcementItem.LimitedToStores = model.SelectedStoreIds.Any();

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(announcementItem);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                    await _storeMappingService.InsertStoreMappingAsync(announcementItem, store.Id);
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

    protected virtual async Task SaveAnnouncementItemAclAsync(AnnouncementItem announcementItem, AnnouncementItemModel model)
    {
        announcementItem.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _announcementItemService.UpdateAnnouncementItemAsync(announcementItem);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(announcementItem);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                    await _aclService.InsertAclRecordAsync(announcementItem, customerRole.Id);
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

    protected virtual async Task UpdateLocalesAsync(AnnouncementItem announcementItem, AnnouncementItemModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(announcementItem,
                x => x.Title,
                localized.Title,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(announcementItem,
                x => x.Description,
                localized.Description,
                localized.LanguageId);
        }
    }

    #endregion

    #region Methods

    #region Configure

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        var model = await _announcementItemModelFactory.PrepareConfigurationModelAsync();

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var announcementSettings = await _settingService.LoadSettingAsync<AnnouncementSettings>(storeScope);
        announcementSettings = model.ToSettings(announcementSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(announcementSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(announcementSettings, x => x.WidgetZone, model.WidgetZone_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(announcementSettings, x => x.ItemSeparator, model.ItemSeparator_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(announcementSettings, x => x.DisplayTypeId, model.DisplayTypeId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(announcementSettings, x => x.AllowCustomersToClose, model.AllowCustomersToClose_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(announcementSettings, x => x.AllowCustomersToMinimize, model.AllowCustomersToMinimize_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion

    #region Announcement item        

    public virtual async Task<IActionResult> Index()
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        var searchModel = _announcementItemModelFactory.PrepareAnnouncementItemSearchModel(new AnnouncementItemSearchModel());
        return View(searchModel);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(AnnouncementItemSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return await AccessDeniedDataTablesJson();

        var model = await _announcementItemModelFactory.PrepareAnnouncementItemListModelAsync(searchModel);
        return Json(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        var model = await _announcementItemModelFactory.PrepareAnnouncementItemModelAsync(new AnnouncementItemModel(), null);
        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Create(AnnouncementItemModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var announcementItem = model.ToEntity<AnnouncementItem>();
            await _announcementItemService.InsertAnnouncementItemAsync(announcementItem);

            //locales
            await UpdateLocalesAsync(announcementItem, model);

            await SaveStoreMappingsAsync(announcementItem, model);

            //ACL (customer roles)
            await SaveAnnouncementItemAclAsync(announcementItem, model);

            //update schedule mappings
            await UpdateScheduleMappingsAsync(model.Schedule, announcementItem, async (announcementItem) => await _announcementItemService.UpdateAnnouncementItemAsync(announcementItem));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Announcement.AnnouncementItems.Created"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = announcementItem.Id });
        }

        model = await _announcementItemModelFactory.PrepareAnnouncementItemModelAsync(new AnnouncementItemModel(), null);

        return View(model);
    }

    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        var announcementItem = await _announcementItemService.GetAnnouncementItemByIdAsync(id);
        if (announcementItem == null)
            return RedirectToAction("List");

        var model = await _announcementItemModelFactory.PrepareAnnouncementItemModelAsync(null, announcementItem);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(AnnouncementItemModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        var announcementItem = await _announcementItemService.GetAnnouncementItemByIdAsync(model.Id);
        if (announcementItem == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            announcementItem = model.ToEntity(announcementItem);

            await _announcementItemService.UpdateAnnouncementItemAsync(announcementItem);

            //locales
            await UpdateLocalesAsync(announcementItem, model);

            await SaveStoreMappingsAsync(announcementItem, model);

            //ACL (customer roles)
            await SaveAnnouncementItemAclAsync(announcementItem, model);

            //update schedule mappings
            await UpdateScheduleMappingsAsync(model.Schedule, announcementItem, async (announcementItem) => await _announcementItemService.UpdateAnnouncementItemAsync(announcementItem));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Announcement.AnnouncementItems.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = announcementItem.Id });
        }

        model = await _announcementItemModelFactory.PrepareAnnouncementItemModelAsync(model, announcementItem);
        return View(model);
    }

    [EditAccess, HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        var announcementItem = await _announcementItemService.GetAnnouncementItemByIdAsync(id);
        if (announcementItem == null)
            return RedirectToAction("List");

        await _announcementItemService.DeleteAnnouncementItemAsync(announcementItem);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Announcement.AnnouncementItems.Deleted"));

        return RedirectToAction("List");
    }

    #endregion

    #region Customer condition mappings

    [HttpPost]
    public virtual async Task<IActionResult> CustomerConditionList(CustomerConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return await AccessDeniedDataTablesJson();

        //try to get a announcementItem with the specified id
        var announcementItem = await _announcementItemService.GetAnnouncementItemByIdAsync(searchModel.EntityId);
        if (announcementItem == null)
            throw new ArgumentException("No announcementItem found with the specified id");

        //prepare model
        var model = await base.CustomerConditionListAsync(searchModel, announcementItem);

        return Json(model);
    }

    public virtual async Task<IActionResult> CustomerConditionDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        //try to get a announcementItem with the specified id
        var announcementItem = await _announcementItemService.GetAnnouncementItemByIdAsync(entityId);
        if (announcementItem == null)
            throw new ArgumentException("No announcementItem found with the specified id");

        await base.CustomerConditionDeleteAsync(id, announcementItem);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> CustomerConditionAddPopup(int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        //try to get a announcementItem with the specified id
        var announcementItem = await _announcementItemService.GetAnnouncementItemByIdAsync(entityId);
        if (announcementItem == null)
            throw new ArgumentException("No announcementItem found with the specified id");

        //prepare model
        var model = await _conditionModelFactory.PrepareAddCustomerToConditionSearchModelAsync(new AddCustomerToConditionSearchModel(), announcementItem);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CustomerConditionAddList(AddCustomerToConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _conditionModelFactory.PrepareAddCustomerToConditionListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CustomerConditionAddPopup(AddCustomerToConditionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
            return AccessDeniedView();

        //try to get a announcementItem with the specified id
        var announcementItem = await _announcementItemService.GetAnnouncementItemByIdAsync(model.EntityId);
        if (announcementItem == null)
            throw new ArgumentException("No announcementItem found with the specified id");

        await base.CustomerConditionAddAsync(model, announcementItem);

        ViewBag.RefreshPage = true;

        return View(new AddCustomerToConditionSearchModel());
    }

    #endregion

    #endregion
}
