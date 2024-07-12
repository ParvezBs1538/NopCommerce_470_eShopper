using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.DMS.Domain
{
    public partial class ShipmentNote : BaseEntity
    {
        public ShipmentNote()
        {
            DisplayToCustomer = false;
            DisplayToShipper = false;
            CreatedOnUtc = DateTime.UtcNow;
            Note = "";
        }

        public int CourierShipmentId { get; set; }

        public int NopShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the note
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a customer can see a note
        /// </summary>
        public bool DisplayToCustomer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a customer can see a note
        /// </summary>
        public bool DisplayToShipper { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order note creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        public int UpdatedByCustomerId { get; set; }
    }
}
