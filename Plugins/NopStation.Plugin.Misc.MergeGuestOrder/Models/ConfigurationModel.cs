using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace NopStation.Plugin.Misc.MergeGuestOrder.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AvailableCheckEmailOptions = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MergeGuestOrder.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MergeGuestOrder.Configuration.Fields.AddNoteToOrderOnMerge")]
        public bool AddNoteToOrderOnMerge { get; set; }
        public bool AddNoteToOrderOnMerge_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MergeGuestOrder.Configuration.Fields.CheckEmailInAddress")]
        public int CheckEmailInAddressId { get; set; }
        public bool CheckEmailInAddressId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCheckEmailOptions { get; set; }
    }
}