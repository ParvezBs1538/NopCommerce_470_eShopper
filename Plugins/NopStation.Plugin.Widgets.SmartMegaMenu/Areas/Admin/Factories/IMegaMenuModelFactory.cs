using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Factories;

public interface IMegaMenuModelFactory
{
    #region Mega menus

    Task<MegaMenuSearchModel> PrepareMegaMenuSearchModelAsync(MegaMenuSearchModel searchModel);

    Task<MegaMenuListModel> PrepareMegaMenuListModelAsync(MegaMenuSearchModel searchModel);

    Task<MegaMenuModel> PrepareMegaMenuModelAsync(MegaMenuModel model, MegaMenu megaMenu, bool excludeProperties = false);

    #endregion

    #region Mega menu items

    Task<MegaMenuItemModel> PrepareMegaMenuItemModelAsync(MegaMenuItemModel model, MegaMenuItem menuItem, MegaMenu megaMenu, bool excludeProperties = false);

    Task<AddCategoryToMegaMenuListModel> PrepareCategoryListModelAsync(AddCategoryToMegaMenuSearchModel searchModel);

    Task<AddManufacturerToMegaMenuListModel> PrepareManufacturerListModelAsync(AddManufacturerToMegaMenuSearchModel searchModel);

    Task<AddVendorToMegaMenuListModel> PrepareVendorListModelAsync(AddVendorToMegaMenuSearchModel searchModel);

    Task<AddTopicToMegaMenuListModel> PrepareTopicListModelAsync(AddTopicToMegaMenuSearchModel searchModel);

    Task<AddProductTagToMegaMenuListModel> PrepareProductTagListModelAsync(AddProductTagToMegaMenuSearchModel searchModel);

    #endregion

    #region Mega menu tree

    Task<IList<MenuTreeItemModel>> PrepareMenuTreeItemsModelAsync(int menuId);

    Task<MenuTreeItemModel> PrepareMenuTreeItemModelAsync(MegaMenuItem menuItem);

    #endregion
}
