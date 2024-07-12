using Nop.Core;

namespace NopStation.Plugin.Misc.TinyPNG.Domain
{
    public partial class PictureInfo: BaseEntity
    {
        public int PictureId { get; set; }

        public int BinaryLength { get; set; }

        public bool Compressed { get; set; }
    }
}
