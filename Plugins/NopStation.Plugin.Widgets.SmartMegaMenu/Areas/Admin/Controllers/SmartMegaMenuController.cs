using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Controllers;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;
using NopStation.Plugin.Widgets.SmartMegaMenu.Services;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Controllers;

public class SmartMegaMenuController : BaseWidgetAdminController
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IMegaMenuModelFactory _megaMenuModelFactory;
    private readonly IMegaMenuService _megaMenuService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IHtmlHelper _htmlHelper;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IVendorService _vendorService;
    private readonly ITopicService _topicService;
    private readonly IProductTagService _productTagService;

    #endregion

    #region Ctor

    public SmartMegaMenuController(IStoreContext storeContext,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IMegaMenuModelFactory megaMenuModelFactory,
        IMegaMenuService megaMenuService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        ILocalizedEntityService localizedEntityService,
        IHtmlHelper htmlHelper,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IVendorService vendorService,
        ITopicService topicService,
        IProductTagService productTagService)
    {
        _storeContext = storeContext;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _megaMenuModelFactory = megaMenuModelFactory;
        _megaMenuService = megaMenuService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _localizedEntityService = localizedEntityService;
        _htmlHelper = htmlHelper;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _vendorService = vendorService;
        _topicService = topicService;
        _productTagService = productTagService;
    }

    #endregion

    #region Utilities

    public async Task SaveStoreMappingsAsync(MegaMenu menu, MegaMenuModel model)
    {
        menu.LimitedToStores = model.SelectedStoreIds.Any();

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(menu);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                    await _storeMappingService.InsertStoreMappingAsync(menu, store.Id);
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

    public async Task UpdateLocalesAsync(MegaMenu menu, MegaMenuModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(menu,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
        }
    }

    public void UpdateOrder(IList<TreeUPdateModel.Child> children, IList<MegaMenuItem> menuItems, int parentItemId, ref int displayOrder, ref bool requiresReload)
    {
        foreach (var c1 in children)
        {
            var menuItem = menuItems.FirstOrDefault(mi => mi.Id == c1.Id);
            if (menuItem == null)
            {
                requiresReload = true;
                continue;
            }

            menuItem.DisplayOrder = displayOrder++;
            menuItem.ParentMenuItemId = parentItemId;

            if (c1.Children.Any())
                UpdateOrder(c1.Children, menuItems, menuItem.Id, ref displayOrder, ref requiresReload);
        }
    }

    protected async Task GetMenuItemsWithChildrenAsync(int menuItemId, IList<MegaMenuItem> menuItems)
    {
        var childrenItems = await _megaMenuService.GetMegaMenuItemsByParentMenuItemIdAsync(menuItemId, true);

        foreach (var item in childrenItems)
        {
            menuItems.Add(item);
            await GetMenuItemsWithChildrenAsync(item.Id, menuItems);
        }
    }

    #endregion

    #region Methods

    #region Configuration

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var megaMenuSettings = await _settingService.LoadSettingAsync<SmartMegaMenuSettings>(storeId);

        var model = megaMenuSettings.ToSettingsModel<ConfigurationModel>();
        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId > 0)
        {
            model.EnableMegaMenu_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, x => x.EnableMegaMenu, storeId);
            model.HideDefaultMenu_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, x => x.HideDefaultMenu, storeId);
            model.MenuItemPictureSize_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, x => x.MenuItemPictureSize, storeId);
        }

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var megaMenuSettings = await _settingService.LoadSettingAsync<SmartMegaMenuSettings>(storeScope);

        megaMenuSettings = model.ToSettings(megaMenuSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, x => x.EnableMegaMenu, model.EnableMegaMenu_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, x => x.HideDefaultMenu, model.HideDefaultMenu_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, x => x.MenuItemPictureSize, model.MenuItemPictureSize_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion

    #region List

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var model = await _megaMenuModelFactory.PrepareMegaMenuSearchModelAsync(new MegaMenuSearchModel());

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> List(MegaMenuSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var model = await _megaMenuModelFactory.PrepareMegaMenuListModelAsync(searchModel);
        return Json(model);
    }

    #endregion

    #region Create/update/delete

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var model = await _megaMenuModelFactory.PrepareMegaMenuModelAsync(new MegaMenuModel(), null);
        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Create(MegaMenuModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var megaMenu = model.ToEntity<MegaMenu>();
            megaMenu.CreatedOnUtc = DateTime.UtcNow;
            megaMenu.UpdatedOnUtc = DateTime.UtcNow;

            await _megaMenuService.InsertMegaMenuAsync(megaMenu);

            await UpdateLocalesAsync(megaMenu, model);
            await SaveStoreMappingsAsync(megaMenu, model);

            await _megaMenuService.UpdateMegaMenuAsync(megaMenu);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.MegaMenus.Created"));

            return continueEditing
                ? RedirectToAction("Edit", new { id = megaMenu.Id })
                : RedirectToAction("List");
        }

        model = await _megaMenuModelFactory.PrepareMegaMenuModelAsync(model, null);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var menu = await _megaMenuService.GetMegaMenuByIdAsync(id);
        if (menu == null || menu.Deleted)
            return RedirectToAction("List");

        var model = await _megaMenuModelFactory.PrepareMegaMenuModelAsync(null, menu);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(MegaMenuModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var menu = await _megaMenuService.GetMegaMenuByIdAsync(model.Id);

        if (menu == null || menu.Deleted)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            menu = model.ToEntity(menu);
            menu.UpdatedOnUtc = DateTime.UtcNow;

            await _megaMenuService.UpdateMegaMenuAsync(menu);

            await UpdateLocalesAsync(menu, model);

            await SaveStoreMappingsAsync(menu, model);

            await _megaMenuService.UpdateMegaMenuAsync(menu);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.MegaMenus.Updated"));

            return continueEditing
                ? RedirectToAction("Edit", new { id = model.Id })
                : RedirectToAction("List");
        }

        model = await _megaMenuModelFactory.PrepareMegaMenuModelAsync(model, menu);
        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var menu = await _megaMenuService.GetMegaMenuByIdAsync(id);
        await _megaMenuService.DeleteMegaMenuAsync(menu);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.MegaMenus.Deleted"));

        return RedirectToAction("List");
    }

    #endregion

    #region Menu Options

    [HttpPost]
    public async Task<ActionResult> GetCategories(AddCategoryToMegaMenuSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var model = await _megaMenuModelFactory.PrepareCategoryListModelAsync(searchModel);
        return Json(model);
    }

    [HttpPost]
    public async Task<ActionResult> GetManufacturers(AddManufacturerToMegaMenuSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var model = await _megaMenuModelFactory.PrepareManufacturerListModelAsync(searchModel);
        return Json(model);
    }

    [HttpPost]
    public async Task<ActionResult> GetVendors(AddVendorToMegaMenuSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var model = await _megaMenuModelFactory.PrepareVendorListModelAsync(searchModel);
        return Json(model);
    }

    [HttpPost]
    public async Task<ActionResult> GetTopics(AddTopicToMegaMenuSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var model = await _megaMenuModelFactory.PrepareTopicListModelAsync(searchModel);
        return Json(model);
    }

    [HttpPost]
    public async Task<ActionResult> GetProductTags(AddProductTagToMegaMenuSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var model = await _megaMenuModelFactory.PrepareProductTagListModelAsync(searchModel);
        return Json(model);
    }

    #endregion

    #region Widget zone mappings

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneList(WidgetZoneSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var megaMenu = await _megaMenuService.GetMegaMenuByIdAsync(searchModel.EntityId);
        if (megaMenu == null || megaMenu.Deleted)
            throw new ArgumentException("No megaMenu found with the specified id");

        var model = await base.WidgetZoneListAsync(searchModel, megaMenu);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneCreate(WidgetZoneModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var megaMenu = await _megaMenuService.GetMegaMenuByIdAsync(model.EntityId);
        if (megaMenu == null || megaMenu.Deleted)
            throw new ArgumentException("No megaMenu found with the specified id");

        await base.WidgetZoneCreateAsync(model, megaMenu);

        return Json(new { Result = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneEdit(WidgetZoneModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var megaMenu = await _megaMenuService.GetMegaMenuByIdAsync(model.EntityId);
        if (megaMenu == null || megaMenu.Deleted)
            throw new ArgumentException("No megaMenu found with the specified id");

        await base.WidgetZoneEditAsync(model, megaMenu);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return await AccessDeniedDataTablesJson();

        var megaMenu = await _megaMenuService.GetMegaMenuByIdAsync(entityId);
        if (megaMenu == null || megaMenu.Deleted)
            throw new ArgumentException("No megaMenu found with the specified id");

        await base.WidgetZoneDeleteAsync(id, megaMenu);

        return new NullJsonResult();
    }

    #endregion

    #region Mega menu tree

    public async Task<IActionResult> GetMenuTree(int menuId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var model = await _megaMenuModelFactory.PrepareMenuTreeItemsModelAsync(menuId);
        return Json(model);
    }

    public async Task<IActionResult> GetNode(int menuItemId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var menuItem = await _megaMenuService.GetMegaMenuItemByIdAsync(menuItemId)
            ?? throw new ArgumentNullException("No menu item found with the specific id");

        var megaMenu = await _megaMenuService.GetMegaMenuByIdAsync(menuItem.MegaMenuId)
            ?? throw new ArgumentNullException("No mega menu found with the specific id");

        var model = await _megaMenuModelFactory.PrepareMegaMenuItemModelAsync(null, menuItem, megaMenu);

        ViewData.TemplateInfo.HtmlFieldPrefix = $"MenuItem_{model.Id}";
        var html = await RenderPartialViewToStringAsync(null, model);

        return Json(new { html = html });
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> CreateMenuItem(MegaMenuItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var megaMenu = await _megaMenuService.GetMegaMenuByIdAsync(model.MegaMenuId)
            ?? throw new ArgumentNullException("No mega menu found with the specific id");

        var existingMenuItems = await _megaMenuService.GetAllMegaMenuItemsAsync(model.MegaMenuId, showHidden: true);
        var displayOrder = existingMenuItems.Count == 0 ? 0 : existingMenuItems.Max(mi => mi.DisplayOrder) + 1;

        var menuItem = new MegaMenuItem
        {
            DisplayOrder = displayOrder,
            MegaMenuId = model.MegaMenuId,
            MenuItemTypeId = model.MenuItemTypeId,
        };

        switch ((MenuItemType)model.MenuItemTypeId)
        {
            case MenuItemType.Category:
                var category = await _categoryService.GetCategoryByIdAsync(model.CategoryId)
                    ?? throw new ArgumentNullException("No category found with the specific id");

                menuItem.CategoryId = model.CategoryId;
                break;
            case MenuItemType.Manufacturer:
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(model.ManufacturerId)
                    ?? throw new ArgumentNullException("No manufacturer found with the specific id");

                menuItem.ManufacturerId = model.ManufacturerId;
                break;
            case MenuItemType.Vendor:
                var vendor = await _vendorService.GetVendorByIdAsync(model.VendorId)
                    ?? throw new ArgumentNullException("No vendor found with the specific id");

                menuItem.VendorId = model.VendorId;
                break;
            case MenuItemType.Topic:
                var topic = await _topicService.GetTopicByIdAsync(model.TopicId)
                    ?? throw new ArgumentNullException("No topic found with the specific id");

                menuItem.Title = string.IsNullOrWhiteSpace(topic?.Title) ? topic?.SystemName : topic?.Title;
                menuItem.TopicId = model.TopicId;
                break;
            case MenuItemType.ProductTag:
                var tag = await _productTagService.GetProductTagByIdAsync(model.ProductTagId)
                    ?? throw new ArgumentNullException("No product tag found with the specific id");

                menuItem.Title = tag.Name;
                menuItem.ProductTagId = model.ProductTagId;
                break;
            case MenuItemType.Page:
                menuItem.PageTypeId = model.PageTypeId;
                break;
            case MenuItemType.CustomLink:
                break;
            default:
                break;
        }

        await _megaMenuService.InsertMegaMenuItemAsync(menuItem);

        var treeModel = await _megaMenuModelFactory.PrepareMenuTreeItemModelAsync(menuItem);

        return Json(new { data = treeModel });
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> CreateCustomLinkMenuItem(MegaMenuItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var megaMenu = await _megaMenuService.GetMegaMenuByIdAsync(model.MegaMenuId)
            ?? throw new ArgumentNullException("No mega menu found with the specific id");

        var existingMenuItems = await _megaMenuService.GetAllMegaMenuItemsAsync(model.MegaMenuId, showHidden: true);
        var displayOrder = existingMenuItems.Count == 0 ? 0 : existingMenuItems.Max(mi => mi.DisplayOrder) + 1;

        var menuItem = new MegaMenuItem
        {
            DisplayOrder = displayOrder,
            MegaMenuId = model.MegaMenuId,
            MenuItemTypeId = (int)MenuItemType.CustomLink,
            Title = model.Title,
            Url = model.Url
        };

        await _megaMenuService.InsertMegaMenuItemAsync(menuItem);

        var treeModel = await _megaMenuModelFactory.PrepareMenuTreeItemModelAsync(menuItem);

        return Json(new { data = treeModel });
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> UpdateMenuItemOrder(TreeUPdateModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var megaMenu = await _megaMenuService.GetMegaMenuByIdAsync(model.MegaMenuId)
            ?? throw new ArgumentNullException("No mega menu found with the specific id");

        var existingMenuItems = await _megaMenuService.GetAllMegaMenuItemsAsync(model.MegaMenuId, showHidden: true);
        var displayOrder = 0;

        var requiresReload = false;
        UpdateOrder(model.Data, existingMenuItems, 0, ref displayOrder, ref requiresReload);

        await _megaMenuService.UpdateMegaMenuItemsAsync(existingMenuItems);

        return Json(new { Result = true, RequiresReload = requiresReload });
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> DeleteMenuItems(int[] ids)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var menuItems = await _megaMenuService.GetMegaMenuItemsByIdAsync(ids);
        if (!menuItems.Any())
            return Json(new { Result = true });

        var allItems = new List<MegaMenuItem>();
        allItems.AddRange(menuItems);

        foreach (var menuItem in menuItems)
            await GetMenuItemsWithChildrenAsync(menuItem.Id, allItems);

        await _megaMenuService.DeleteMegaMenuItemsAsync(allItems);

        return Json(new { Result = true });
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> UpdateMenuItem(MegaMenuItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var menuItem = await _megaMenuService.GetMegaMenuItemByIdAsync(model.Id)
            ?? throw new ArgumentNullException("No menu item found with the specific id");

        if (ModelState.IsValid)
        {
            menuItem = model.ToEntity(menuItem);
            await _megaMenuService.UpdateMegaMenuItemAsync(menuItem);

            var treeModel = await _megaMenuModelFactory.PrepareMenuTreeItemModelAsync(menuItem);

            return Json(new { Result = true, Node = treeModel });
        }

        var errors = ModelState.Values.SelectMany(x => x.Errors.Select(c => c.ErrorMessage)).ToList();
        return Json(new { Errors = errors });
    }

    #endregion

    #endregion
}
