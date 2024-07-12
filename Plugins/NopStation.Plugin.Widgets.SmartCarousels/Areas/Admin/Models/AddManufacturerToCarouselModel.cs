using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public partial record AddManufacturerToCarouselModel : BaseNopModel
{
    #region Ctor

    public AddManufacturerToCarouselModel()
    {
        SelectedManufacturerIds = new List<int>();
    }
    #endregion

    #region Properties

    public int CarouselId { get; set; }

    public IList<int> SelectedManufacturerIds { get; set; }

    #endregion
}