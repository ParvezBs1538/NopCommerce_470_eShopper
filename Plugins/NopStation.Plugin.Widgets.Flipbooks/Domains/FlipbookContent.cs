using Nop.Core;

namespace NopStation.Plugin.Widgets.Flipbooks.Domains
{
    public class FlipbookContent : BaseEntity
    {
        public int FlipbookId { get; set; }

        public int ImageId { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsImage { get; set; }

        public string RedirectUrl { get; set; }
    }
}
