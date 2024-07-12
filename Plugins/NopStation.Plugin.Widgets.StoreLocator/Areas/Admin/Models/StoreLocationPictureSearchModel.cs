using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models
{
    public partial record StoreLocationPictureSearchModel : BaseSearchModel
    {
        public int StoreLocationId { get; set; }
    }
}