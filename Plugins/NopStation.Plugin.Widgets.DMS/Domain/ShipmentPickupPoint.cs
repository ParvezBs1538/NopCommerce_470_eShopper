using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Widgets.DMS.Domain
{
    public partial class ShipmentPickupPoint : BaseEntity, ISoftDeletedEntity
    {
        /// <summary>
        /// Gets or sets a name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description
        /// </summary>
        public string Description { get; set; }

        public string OpeningHours { get; set; }

        /// <summary>
        /// Gets or sets an address identifier
        /// </summary>

        public int AddressId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        /// <summary>
        /// Gets or sets a display order
        /// </summary>
        public int DisplayOrder { get; set; }

        public bool Active { get; set; }

        public bool Deleted { get; set; }
    }
}
