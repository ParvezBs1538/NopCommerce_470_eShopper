using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Services;

public interface IMegaMenuService
{
    #region Mega menu

    Task<IPagedList<MegaMenu>> GetAllMegaMenusAsync(string keyword = null, int storeId = 0, string widgetZone = null,
       bool showHidden = false, bool? overridePublished = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<MegaMenu> GetMegaMenuByIdAsync(int megaMenuId);

    Task InsertMegaMenuAsync(MegaMenu megaMenu);

    Task UpdateMegaMenuAsync(MegaMenu megaMenu);

    Task DeleteMegaMenuAsync(MegaMenu megaMenu);

    #endregion

    #region Mega menu Items

    Task<MegaMenuItem> GetMegaMenuItemByIdAsync(int megaMenuItemId);

    Task<IList<MegaMenuItem>> GetMegaMenuItemsByIdAsync(int[] megaMenuItemIds);

    Task UpdateMegaMenuItemAsync(MegaMenuItem megaMenuItem);

    Task UpdateMegaMenuItemsAsync(IList<MegaMenuItem> megaMenuItems);

    Task InsertMegaMenuItemAsync(MegaMenuItem megaMenuItem);

    Task DeleteMegaMenuItemAsync(MegaMenuItem megaMenuItem);

    Task DeleteMegaMenuItemsAsync(IList<MegaMenuItem> megaMenuItems);

    Task<IList<MegaMenuItem>> GetAllMegaMenuItemsAsync(int megaMenuId, MenuItemType? menuItemType = null, bool showHidden = false);

    Task<IList<MegaMenuItem>> GetMegaMenuItemsByParentMenuItemIdAsync(int parentMenuItemId, bool showHidden = false);

    #endregion
}
