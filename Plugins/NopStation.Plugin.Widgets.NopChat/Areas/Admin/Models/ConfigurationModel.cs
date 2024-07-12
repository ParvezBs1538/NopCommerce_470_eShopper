using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.NopChat.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.Logo")]

        public int Logo { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnMonday")]
        public bool OpenOnMonday { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnTuesday")]
        public bool OpenOnTuesday { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnWednesday")]
        public bool OpenOnWednesday { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnThursday")]
        public bool OpenOnThursday { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnFriday")]
        public bool OpenOnFriday { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnSaturday")]
        public bool OpenOnSaturday { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnSunday")]
        public bool OpenOnSunday { get; set; }


        public bool Logo_OverrideForStore { get; set; }
        public bool OpenOnMonday_OverrideForStore { get; set; }
        public bool OpenOnTuesday_OverrideForStore { get; set; }
        public bool OpenOnWednesday_OverrideForStore { get; set; }
        public bool OpenOnThursday_OverrideForStore { get; set; }
        public bool OpenOnFriday_OverrideForStore { get; set; }
        public bool OpenOnSaturday_OverrideForStore { get; set; }
        public bool OpenOnSunday_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
