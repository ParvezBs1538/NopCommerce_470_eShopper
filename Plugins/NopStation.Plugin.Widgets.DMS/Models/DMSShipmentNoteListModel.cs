using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public class DMSShipmentNoteListModel
    {
        public DMSShipmentNoteListModel()
        {
            ShipmentNotes = new List<DMSShipmentNoteModel> { };
        }

        public IList<DMSShipmentNoteModel> ShipmentNotes { get; set; }
    }
}
