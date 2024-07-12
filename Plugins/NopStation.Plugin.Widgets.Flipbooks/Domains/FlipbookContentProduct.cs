using Nop.Core;

namespace NopStation.Plugin.Widgets.Flipbooks.Domains
{
    public class FlipbookContentProduct : BaseEntity
    {
        public int FlipbookContentId { get; set; }

        public int ProductId { get; set; }

        public int DisplayOrder { get; set; }
    }
}
