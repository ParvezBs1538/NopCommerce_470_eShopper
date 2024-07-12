using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public class DeliverFailedReasonTypesResponseModel
    {
        public DeliverFailedReasonTypesResponseModel()
        {
            AvailableDeliverFailedReasonTypes = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableDeliverFailedReasonTypes { get; set; }
    }
}
