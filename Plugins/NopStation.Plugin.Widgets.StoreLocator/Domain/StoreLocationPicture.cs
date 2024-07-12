using Nop.Core;

namespace NopStation.Plugin.Widgets.StoreLocator.Domain
{
    public class StoreLocationPicture : BaseEntity
    {
        public int StoreLocationId { get; set; }

        public int PictureId { get; set; }

        public int DisplayOrder { get; set; }
    }
}
