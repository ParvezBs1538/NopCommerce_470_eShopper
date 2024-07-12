using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.OCarousels.Domains
{
    public partial class OCarousel : BaseEntity, ILocalizedEntity, IStoreMappingSupported, ISoftDeletedEntity
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public bool DisplayTitle { get; set; }

        public bool Active { get; set; }

        public int DataSourceTypeId { get; set; }

        public bool ShowBackgroundPicture { get; set; }

        public int BackgroundPictureId { get; set; }

        public string CustomUrl { get; set; }

        public int NumberOfItemsToShow { get; set; }

        public int WidgetZoneId { get; set; }

        public int DisplayOrder { get; set; }

        public bool AutoPlay { get; set; }

        public string CustomCssClass { get; set; }

        public bool Loop { get; set; }

        public int StartPosition { get; set; }

        public bool Center { get; set; }

        public bool Nav { get; set; }

        public bool LazyLoad { get; set; }

        public int LazyLoadEager { get; set; }

        public int AutoPlayTimeout { get; set; }

        public bool AutoPlayHoverPause { get; set; }

        public bool Deleted { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public bool LimitedToStores { get; set; }

        public DataSourceTypeEnum DataSourceTypeEnum
        {
            get => (DataSourceTypeEnum)DataSourceTypeId;
            set => DataSourceTypeId = (int)value;
        }
    }
}
