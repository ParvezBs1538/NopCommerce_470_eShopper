using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models
{
    public record VendorCampaignModel : BaseNopModel
    {
        public VendorCampaignModel()
        {
            SelectedIds = new List<int>();
        }
        public bool SendToAll { get; set; }
        public IList<int> SelectedIds { get; set; }
        public string ToName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        [UIHint("DateTimeNullable")]
        public DateTime? SendingDate { get; set; }

    }
}
