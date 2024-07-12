using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models
{
    public record FlipbookContentProductModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContentProducts.Fields.Content")]
        public int FlipbookContentId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContentProducts.Fields.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContentProducts.Fields.Product")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContentProducts.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}
