using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public record DMSShipmentNoteModel : BaseNopEntityModel
    {
        public int NopShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the note
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order note creation
        /// </summary>
        public DateTime CreatedOn { get; set; }

        public int UpdatedByCustomerId { get; set; }

        public string UpdatedByCustomerEmail { get; set; }
    }
}
