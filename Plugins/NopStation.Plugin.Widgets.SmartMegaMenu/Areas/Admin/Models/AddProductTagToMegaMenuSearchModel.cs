using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public partial record AddProductTagToMegaMenuSearchModel : BaseSearchModel
{
    #region Properties

    public int MegaMenuId { get; set; }

    #endregion
}