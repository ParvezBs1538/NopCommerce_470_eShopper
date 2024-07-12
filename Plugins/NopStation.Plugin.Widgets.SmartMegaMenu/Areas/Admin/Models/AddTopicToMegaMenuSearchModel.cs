using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public partial record AddTopicToMegaMenuSearchModel : BaseSearchModel
{
    #region Properties

    public int MegaMenuId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.Topics.List.SearchKeywords")]
    public string SearchKeywords { get; set; }

    #endregion
}