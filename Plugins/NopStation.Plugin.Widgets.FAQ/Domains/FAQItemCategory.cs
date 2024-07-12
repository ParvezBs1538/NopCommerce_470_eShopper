using Nop.Core;

namespace NopStation.Plugin.Widgets.FAQ.Domains
{
    public class FAQItemCategory : BaseEntity
    {
        public int FAQItemId { get; set; }

        public int FAQCategoryId { get; set; }
    }
}
