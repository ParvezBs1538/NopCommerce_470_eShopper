using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Models
{
    public record SEOModel : BaseNopModel
    {
        public int EntityId { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public int EntityTypeId { get; set; }
        public bool EnableListGeneration { get; set; }

    }
}
