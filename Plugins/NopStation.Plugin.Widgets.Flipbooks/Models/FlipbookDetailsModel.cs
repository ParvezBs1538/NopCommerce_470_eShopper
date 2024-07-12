using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.Flipbooks.Models
{
    public record FlipbookDetailsModel : BaseNopEntityModel
    {
        public FlipbookDetailsModel()
        {
            Contents = new List<FlipbookContentModel>();
        }

        public string Name { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }

        public List<FlipbookContentModel> Contents { get; set; }
    }
}
