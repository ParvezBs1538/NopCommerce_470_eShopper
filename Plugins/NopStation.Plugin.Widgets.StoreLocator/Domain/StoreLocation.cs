using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.StoreLocator.Domain
{
    public class StoreLocation : BaseEntity, ISoftDeletedEntity, ILocalizedEntity, IStoreMappingSupported, ISlugSupported
    {
        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string OpeningHours { get; set; }

        public int AddressId { get; set; }

        public string FullAddress { get; set; }

        public string GoogleMapLocation { get; set; }

        public int DisplayOrder { get; set; }

        public decimal PickupFee { get; set; }

        public bool IsPickupPoint { get; set; }

        public bool Active { get; set; }

        public bool Deleted { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public bool LimitedToStores { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }
    }
}
