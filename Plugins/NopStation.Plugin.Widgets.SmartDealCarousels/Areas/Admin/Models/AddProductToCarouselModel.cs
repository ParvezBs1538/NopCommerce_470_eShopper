using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;

public partial record AddProductToCarouselModel : BaseNopModel
{
    #region Ctor

    public AddProductToCarouselModel()
    {
        SelectedProductIds = new List<int>();
    }
    #endregion

    #region Properties

    public int CarouselId { get; set; }

    public IList<int> SelectedProductIds { get; set; }

    #endregion
}