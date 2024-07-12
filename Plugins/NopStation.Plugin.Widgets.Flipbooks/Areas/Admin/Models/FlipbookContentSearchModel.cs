using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models
{
    public record FlipbookContentSearchModel : BaseSearchModel
    {
        public int FlipbookId { get; set; }
    }
}
