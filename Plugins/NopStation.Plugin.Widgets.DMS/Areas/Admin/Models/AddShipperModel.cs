using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Models
{
    public record AddShipperModel
    {
        public AddShipperModel()
        {
            SelectedCustomerIds = new List<int>();
        }

        public IList<int> SelectedCustomerIds { get; set; }
    }
}
