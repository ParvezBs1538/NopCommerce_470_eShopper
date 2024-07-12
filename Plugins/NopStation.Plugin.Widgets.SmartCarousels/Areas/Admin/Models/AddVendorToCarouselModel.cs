using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public partial record AddVendorToCarouselModel : BaseNopModel
{
    #region Ctor

    public AddVendorToCarouselModel()
    {
        SelectedVendorIds = new List<int>();
    }
    #endregion

    #region Properties

    public int CarouselId { get; set; }

    public IList<int> SelectedVendorIds { get; set; }

    #endregion
}