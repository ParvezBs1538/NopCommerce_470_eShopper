using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record AddCategoryToBadgeModel : BaseNopModel
{
    #region Ctor

    public AddCategoryToBadgeModel()
    {
        SelectedCategoryIds = new List<int>();
    }

    #endregion

    #region Properties

    public int BadgeId { get; set; }

    public IList<int> SelectedCategoryIds { get; set; }

    #endregion
}