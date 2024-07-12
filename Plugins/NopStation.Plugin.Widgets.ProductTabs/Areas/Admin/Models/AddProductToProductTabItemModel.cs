using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models
{
    /// <summary>
    /// Represents a product model to add to the discount
    /// </summary>
    public partial record AddProductToProductTabItemModel : BaseNopModel
    {
        #region Ctor

        public AddProductToProductTabItemModel()
        {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        public int ProductTabItemId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }
}