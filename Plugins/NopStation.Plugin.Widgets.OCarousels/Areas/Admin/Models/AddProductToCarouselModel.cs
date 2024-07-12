using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models
{
    /// <summary>
    /// Represents a product model to add to the discount
    /// </summary>
    public partial record AddProductToCarouselModel : BaseNopModel
    {
        #region Ctor

        public AddProductToCarouselModel()
        {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        public int OCarouselId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }
}