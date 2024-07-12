using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record AddVendorToBadgeModel : BaseNopModel
{
    #region Ctor

    public AddVendorToBadgeModel()
    {
        SelectedVendorIds = new List<int>();
    }

    #endregion

    #region Properties

    public int BadgeId { get; set; }

    public IList<int> SelectedVendorIds { get; set; }

    #endregion
}