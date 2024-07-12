using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models
{
    /// <summary>
    /// Represents a product model to add to the discount
    /// </summary>
    public partial record AddProductToFlipbookContentModel : BaseNopModel
    {
        #region Ctor

        public AddProductToFlipbookContentModel()
        {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        public int FlipbookContentId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }
}