using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Models
{
    public record ShipperSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.DMS.Shippers.List.SearchEmail")]
        public string SearchEmail { get; set; }
    }
}
