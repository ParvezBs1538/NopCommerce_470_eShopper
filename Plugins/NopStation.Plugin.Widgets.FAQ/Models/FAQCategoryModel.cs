using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.FAQ.Models
{
    public record FAQCategoryModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
