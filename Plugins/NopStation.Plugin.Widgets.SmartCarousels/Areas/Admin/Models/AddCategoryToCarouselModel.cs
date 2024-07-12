using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public partial record AddCategoryToCarouselModel : BaseNopModel
{
    #region Ctor

    public AddCategoryToCarouselModel()
    {
        SelectedCategoryIds = new List<int>();
    }
    #endregion

    #region Properties

    public int CarouselId { get; set; }

    public IList<int> SelectedCategoryIds { get; set; }

    #endregion
}