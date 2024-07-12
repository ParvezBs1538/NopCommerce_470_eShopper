using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record AddProductToBadgeModel : BaseNopModel
{
    #region Ctor

    public AddProductToBadgeModel()
    {
        SelectedProductIds = new List<int>();
    }

    #endregion

    #region Properties

    public int BadgeId { get; set; }

    public IList<int> SelectedProductIds { get; set; }

    #endregion
}