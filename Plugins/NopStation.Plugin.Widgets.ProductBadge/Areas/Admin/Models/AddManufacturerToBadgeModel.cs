using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record AddManufacturerToBadgeModel : BaseNopModel
{
    #region ctor

    public AddManufacturerToBadgeModel()
    {
        SelectedManufacturerIds = new List<int>();
    }

    #endregion

    #region Properties

    public int BadgeId { get; set; }

    public IList<int> SelectedManufacturerIds { get; set; }

    #endregion
}