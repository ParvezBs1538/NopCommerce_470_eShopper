using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.CRM.Zoho.Areas.Admin.Models
{
    public record SyncModel : BaseNopModel
    {
        public SyncModel()
        {
            SyncTables = new List<string>();
            AvailableTables = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Sync.Fields.SyncTables")]
        public IList<string> SyncTables { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Sync.Fields.SyncType")]
        public int SyncType { get; set; }

        public IList<SelectListItem> AvailableTables { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public bool CanSync { get; set; }

        public string OAuthUrl { get; set; }
    }
}
