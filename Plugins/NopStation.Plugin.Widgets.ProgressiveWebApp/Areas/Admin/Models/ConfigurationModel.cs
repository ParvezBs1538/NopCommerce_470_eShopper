using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableDisplayTypes = new List<SelectListItem>();
            AvailableUnits = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.VapidSubjectEmail")]
        public string VapidSubjectEmail { get; set; }
        public bool VapidSubjectEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.VapidPublicKey")]
        public string VapidPublicKey { get; set; }
        public bool VapidPublicKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.VapidPrivateKey")]
        public string VapidPrivateKey { get; set; }
        public bool VapidPrivateKey_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.DisableSilent")]
        public bool DisableSilent { get; set; }
        public bool DisableSilent_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.Vibration")]
        public string Vibration { get; set; }
        public bool Vibration_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.SoundFileUrl")]
        public string SoundFileUrl { get; set; }
        public bool SoundFileUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.DefaultIconId")]
        [UIHint("Picture")]
        public int DefaultIconId { get; set; }
        public bool DefaultIconId_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.ManifestName")]
        public string ManifestName { get; set; }
        public bool ManifestName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.ManifestShortName")]
        public string ManifestShortName { get; set; }
        public bool ManifestShortName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.ManifestThemeColor")]
        public string ManifestThemeColor { get; set; }
        public bool ManifestThemeColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.ManifestBackgroundColor")]
        public string ManifestBackgroundColor { get; set; }
        public bool ManifestBackgroundColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.ManifestDisplay")]
        public string ManifestDisplay { get; set; }
        public bool ManifestDisplay_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableDisplayTypes { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.ManifestStartUrl")]
        public string ManifestStartUrl { get; set; }
        public bool ManifestStartUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.ManifestAppScope")]
        public string ManifestAppScope { get; set; }
        public bool ManifestAppScope_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.ManifestPictureId")]
        [UIHint("Picture")]
        public int ManifestPictureId { get; set; }
        public bool ManifestPictureId_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.AbandonedCartCheckingOffset")]
        public int AbandonedCartCheckingOffset { get; set; }
        public bool AbandonedCartCheckingOffset_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.Configuration.Fields.UnitTypeId")]
        public int UnitTypeId { get; set; }
        public bool UnitTypeId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableUnits { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
