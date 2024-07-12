using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.Product360View.Models
{
    public partial record Picture360SearchModel : BaseSearchModel
    {
        #region Properties

        public int ProductId { get; set; }
        public bool IsPanorama { get; set; }

        #endregion
    }
}
