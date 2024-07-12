using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Topics;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;
using NopStation.Plugin.Widgets.SmartMegaMenu.Helpers;
using NopStation.Plugin.Widgets.SmartMegaMenu.Services;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Factories;

public class MegaMenuModelFactory : IMegaMenuModelFactory
{
    #region Fields

    private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ILocalizationService _localizatinService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IMegaMenuService _megaMenuService;
    private readonly ICategoryService _categoryService;
    private readonly ILocalizationService _localizationService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IVendorService _vendorService;
    private readonly ITopicService _topicService;
    private readonly IProductTagService _productTagService;
    private readonly IWidgetZoneModelFactory _widgetZoneModelFactory;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;


    #endregion

    #region Ctor

    public MegaMenuModelFactory(IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        ILocalizedModelFactory localizedModelFactory,
        IBaseAdminModelFactory baseAdminModelFactory,
        ILocalizationService localizatinService,
        IDateTimeHelper dateTimeHelper,
        IMegaMenuService menuService,
        ICategoryService categoryService,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        IVendorService vendorService,
        ITopicService topicService,
        IProductTagService productTagService,
        IWidgetZoneModelFactory widgetZoneModelFactory,
        IAclSupportedModelFactory aclSupportedModelFactory)
    {
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _localizedModelFactory = localizedModelFactory;
        _baseAdminModelFactory = baseAdminModelFactory;
        _localizatinService = localizatinService;
        _dateTimeHelper = dateTimeHelper;
        _megaMenuService = menuService;
        _categoryService = categoryService;
        _localizationService = localizationService;
        _manufacturerService = manufacturerService;
        _vendorService = vendorService;
        _topicService = topicService;
        _productTagService = productTagService;
        _widgetZoneModelFactory = widgetZoneModelFactory;
        _aclSupportedModelFactory = aclSupportedModelFactory;
    }

    #endregion

    #region Utilities

    protected async Task PrepareTreeItemsModelAsync(IList<MenuTreeItemModel> model, IList<MegaMenuItem> existingMenuItems, int parentItemId)
    {
        var menuItems = existingMenuItems.Where(mi => mi.ParentMenuItemId == parentItemId).ToList();

        foreach (var item in menuItems)
        {
            var m = new MenuTreeItemModel()
            {
                Id = item.Id,
                Name = (await GetMenuItemTitleAsync(item)).Title
            };

            await PrepareTreeItemsModelAsync(m.Children, existingMenuItems, item.Id);
            model.Add(m);
        }
    }

    protected async Task<(string Title, string Name)> GetMenuItemTitleAsync(MegaMenuItem item)
    {
        string title, name;
        switch (item.MenuItemType)
        {
            case MenuItemType.Category:
                var category = await _categoryService.GetCategoryByIdAsync(item.CategoryId);
                title = name = category?.Name;
                break;
            case MenuItemType.Manufacturer:
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(item.ManufacturerId);
                title = name = manufacturer?.Name;
                break;
            case MenuItemType.Vendor:
                var vendor = await _vendorService.GetVendorByIdAsync(item.VendorId);
                title = name = vendor?.Name;
                break;
            case MenuItemType.Page:
                title = await _localizatinService.GetLocalizedEnumAsync(item.PageType, 0);
                name = null;
                break;
            case MenuItemType.Topic:
                title = item.Title;

                var topic = await _topicService.GetTopicByIdAsync(item.TopicId);
                name = string.IsNullOrWhiteSpace(topic?.Title) ? topic?.SystemName : topic?.Title;
                break;
            case MenuItemType.ProductTag:
                title = item.Title;

                var tag = await _productTagService.GetProductTagByIdAsync(item.ProductTagId);
                name = tag?.Name;
                break;
            case MenuItemType.CustomLink:
            default:
                title = item.Title;
                name = null;
                break;
        }

        title = string.IsNullOrWhiteSpace(title) ? "[]" : title;

        return (title, name);
    }

    protected async Task PrepareWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        //prepare available widget zones
        var availableWidgetZoneItems = SmartMegaMenuHelper.GetWidgetZoneSelectList();
        foreach (var widgetZoneItem in availableWidgetZoneItems)
        {
            items.Add(widgetZoneItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = ""
            });
    }

    protected async Task PrepareActiveOptionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        items.Add(new SelectListItem()
        {
            Text = await _localizatinService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchActive.Active"),
            Value = "1"
        });
        items.Add(new SelectListItem()
        {
            Text = await _localizatinService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchActive.Inactive"),
            Value = "2"
        });

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizatinService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PreparePageTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availablePageTypeItems = await PageType.HomePage.ToSelectListAsync(false);
        foreach (var typeItem in availablePageTypeItems)
        {
            items.Add(typeItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareViewTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableViewTypeItems = await ViewType.GridView.ToSelectListAsync(false);
        foreach (var typeItem in availableViewTypeItems)
        {
            items.Add(typeItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected Task PrepareCategoryToMegaMenuSearchModelAsync(AddCategoryToMegaMenuSearchModel searchModel, MegaMenu megaMenu)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.MegaMenuId = megaMenu?.Id ?? 0;

        return Task.CompletedTask;
    }

    protected Task PrepareManufacturerToMegaMenuSearchModelAsync(AddManufacturerToMegaMenuSearchModel searchModel, MegaMenu megaMenu)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.MegaMenuId = megaMenu?.Id ?? 0;

        return Task.CompletedTask;
    }

    protected Task PrepareVendorToMegaMenuSearchModelAsync(AddVendorToMegaMenuSearchModel searchModel, MegaMenu megaMenu)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.MegaMenuId = megaMenu?.Id ?? 0;

        return Task.CompletedTask;
    }

    protected Task PrepareProductTagToMegaMenuSearchModelAsync(AddProductTagToMegaMenuSearchModel searchModel, MegaMenu megaMenu)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.MegaMenuId = megaMenu?.Id ?? 0;

        return Task.CompletedTask;
    }

    protected Task PrepareTopicToMegaMenuSearchModelAsync(AddTopicToMegaMenuSearchModel searchModel, MegaMenu megaMenu)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.MegaMenuId = megaMenu?.Id ?? 0;

        return Task.CompletedTask;
    }

    #endregion

    #region Methods

    #region Mega menus

    public virtual async Task<MegaMenuSearchModel> PrepareMegaMenuSearchModelAsync(MegaMenuSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        await PrepareWidgetZonesAsync(searchModel.AvailableWidgetZones, true);
        await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions, true);

        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        return searchModel;
    }

    public async Task<MegaMenuListModel> PrepareMegaMenuListModelAsync(MegaMenuSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter comments
        var overridePublished = searchModel.SearchActiveId == 0 ? null : (bool?)(searchModel.SearchActiveId == 1);

        var menus = await _megaMenuService.GetAllMegaMenusAsync(
            showHidden: true,
            overridePublished: overridePublished,
            keyword: searchModel.SearchKeyword,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new MegaMenuListModel().PrepareToGridAsync(searchModel, menus, () =>
        {
            return menus.SelectAwait(async menu =>
            {
                return await PrepareMegaMenuModelAsync(null, menu, true);
            });
        });

        return model;
    }

    public async Task<MegaMenuModel> PrepareMegaMenuModelAsync(MegaMenuModel model, MegaMenu megaMenu, bool excludeProperties = false)
    {
        Func<MegaMenuLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (megaMenu != null)
        {
            if (model == null)
            {
                model = megaMenu.ToModel<MegaMenuModel>();

                if (!excludeProperties)
                {
                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.Name = await _localizatinService.GetLocalizedAsync(megaMenu, entity => entity.Name, languageId, false, false);
                    };
                }
            }

            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(megaMenu.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(megaMenu.UpdatedOnUtc, DateTimeKind.Utc);
        }

        if (!excludeProperties)
        {
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, megaMenu, excludeProperties);

            await PrepareViewTypesAsync(model.AvailableViewTypes, false);
            await PreparePageTypesAsync(model.AvailablePageTypes, false);

            //prepare model widget zone mappings
            await _widgetZoneModelFactory.PrepareWidgetZoneMappingSearchModelAsync(model, megaMenu);
            await _widgetZoneModelFactory.PrepareAddWidgetZoneMappingModelAsync(model, megaMenu, true, SmartMegaMenuHelper.GetCustomWidgetZones());

            await PrepareMegaMenuItemModelAsync(model.AddCustomLinkItemModel, null, megaMenu, true);
            await PrepareCategoryToMegaMenuSearchModelAsync(model.AddCategoryToMegaMenuSearchModel, megaMenu);
            await PrepareManufacturerToMegaMenuSearchModelAsync(model.AddManufacturerToMegaMenuSearchModel, megaMenu);
            await PrepareVendorToMegaMenuSearchModelAsync(model.AddVendorToMegaMenuSearchModel, megaMenu);
            await PrepareProductTagToMegaMenuSearchModelAsync(model.AddProductTagToMegaMenuSearchModel, megaMenu);
            await PrepareTopicToMegaMenuSearchModelAsync(model.AddTopicToMegaMenuSearchModel, megaMenu);
        }

        if (megaMenu == null)
        {
            model.Active = true;
        }

        return model;
    }

    #endregion

    #region Mega menu items

    public async Task<MegaMenuItemModel> PrepareMegaMenuItemModelAsync(MegaMenuItemModel model, MegaMenuItem menuItem, MegaMenu megaMenu, bool excludeProperties = false)
    {
        Func<MegaMenuItemLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (menuItem != null)
        {
            if (model == null)
            {
                model = menuItem.ToModel<MegaMenuItemModel>();
                model.Name = (await GetMenuItemTitleAsync(menuItem)).Name;

                if (menuItem.MenuItemType == MenuItemType.Category)
                {
                    var subCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(menuItem.CategoryId, showHidden: true);

                    if (subCategories.Count > 0)
                    {
                        model.CanIncludeSubCategories = true;

                        var existingMenuItems = (await _megaMenuService.GetAllMegaMenuItemsAsync(menuItem.MegaMenuId, MenuItemType.Category, true))
                            .Where(mi => mi.ParentMenuItemId == menuItem.Id)
                            .ToList();

                        foreach (var item in existingMenuItems)
                        {
                            if (subCategories.Any(c => c.Id == item.CategoryId))
                            {
                                model.CanExcludeSubCategories = true;
                                break;
                            }
                        }
                    }
                }

                if (!excludeProperties)
                {
                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.Title = await _localizatinService.GetLocalizedAsync(menuItem, entity => entity.Title, languageId, false, false);
                        locale.Url = await _localizatinService.GetLocalizedAsync(menuItem, entity => entity.Url, languageId, false, false);
                    };
                }
            }
        }

        if (menuItem == null)
        {
            model.MegaMenuId = megaMenu?.Id ?? 0;
        }

        if (!excludeProperties)
        {
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, menuItem, excludeProperties);
        }

        return model;
    }

    public async Task<AddCategoryToMegaMenuListModel> PrepareCategoryListModelAsync(AddCategoryToMegaMenuSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var categories = await _categoryService.GetAllCategoriesAsync(categoryName: searchModel.SearchCategoryName,
            showHidden: true,
            storeId: 0,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new AddCategoryToMegaMenuListModel().PrepareToGridAsync(searchModel, categories, () =>
        {
            return categories.SelectAwait(async category =>
            {
                return new CategoryModel
                {
                    Id = category.Id,
                    Name = await _categoryService.GetFormattedBreadCrumbAsync(category),
                };
            });
        });

        return model;
    }

    public async Task<AddManufacturerToMegaMenuListModel> PrepareManufacturerListModelAsync(AddManufacturerToMegaMenuSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(manufacturerName: searchModel.SearchManufacturerName,
            showHidden: true,
            storeId: 0,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = new AddManufacturerToMegaMenuListModel().PrepareToGrid(searchModel, manufacturers, () =>
        {
            return manufacturers.Select(manufacturer =>
            {
                return new ManufacturerModel
                {
                    Id = manufacturer.Id,
                    Name = manufacturer.Name,
                };
            });
        });

        return model;
    }

    public async Task<AddVendorToMegaMenuListModel> PrepareVendorListModelAsync(AddVendorToMegaMenuSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var vendors = await _vendorService.GetAllVendorsAsync(name: searchModel.SearchName,
            email: searchModel.SearchEmail,
            showHidden: true,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = new AddVendorToMegaMenuListModel().PrepareToGrid(searchModel, vendors, () =>
        {
            return vendors.Select(vendor =>
            {
                return new VendorModel
                {
                    Id = vendor.Id,
                    Name = vendor.Name,
                };
            });
        });

        return model;
    }

    public async Task<AddTopicToMegaMenuListModel> PrepareTopicListModelAsync(AddTopicToMegaMenuSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get topics
        var topics = (await _topicService.GetAllTopicsAsync(showHidden: true,
            keywords: searchModel.SearchKeywords,
            storeId: 0,
            ignoreAcl: true)).ToPagedList(searchModel);

        var model = new AddTopicToMegaMenuListModel().PrepareToGrid(searchModel, topics, () =>
        {
            return topics.Select(topic =>
            {
                return new TopicModel
                {
                    Id = topic.Id,
                    Title = string.IsNullOrWhiteSpace(topic.Title) ? topic.SystemName : topic.Title,
                };
            });
        });

        return model;
    }

    public async Task<AddProductTagToMegaMenuListModel> PrepareProductTagListModelAsync(AddProductTagToMegaMenuSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var tags = await _productTagService.GetAllProductTagsAsync();
        var tagsPaged = tags.ToPagedList(searchModel);

        var model = new AddProductTagToMegaMenuListModel().PrepareToGrid(searchModel, tagsPaged, () =>
        {
            return tagsPaged.Select(tag =>
            {
                return new ProductTagModel
                {
                    Id = tag.Id,
                    Name = tag.Name,
                };
            });
        });

        return model;
    }

    #endregion

    #region Mega menu tree

    public async Task<IList<MenuTreeItemModel>> PrepareMenuTreeItemsModelAsync(int menuId)
    {
        var existingMenuItems = await _megaMenuService.GetAllMegaMenuItemsAsync(menuId, showHidden: true);

        var model = new List<MenuTreeItemModel>();
        await PrepareTreeItemsModelAsync(model, existingMenuItems, 0);

        return model;
    }

    public async Task<MenuTreeItemModel> PrepareMenuTreeItemModelAsync(MegaMenuItem menuItem)
    {
        if (menuItem == null)
            throw new ArgumentNullException(nameof(menuItem));

        var model = new MenuTreeItemModel()
        {
            Id = menuItem.Id,
            Name = (await GetMenuItemTitleAsync(menuItem)).Title
        };

        return model;
    }

    #endregion

    #endregion
}
