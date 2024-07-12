using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models
{
    public record FlipbookContentProductSearchModel : BaseSearchModel
    {
        public int FlipbookContentId { get; set; }
    }
}
