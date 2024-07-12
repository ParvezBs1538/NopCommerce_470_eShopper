using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableDistanceCalculationMethods = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.GoogleMapApiKey")]
        public string GoogleMapApiKey { get; set; }
        public bool GoogleMapApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.SortPickupPointsByDistance")]
        public bool SortPickupPointsByDistance { get; set; }
        public bool SortPickupPointsByDistance_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.DistanceCalculationMethodId")]
        public int DistanceCalculationMethodId { get; set; }
        public bool DistanceCalculationMethodId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableDistanceCalculationMethods { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.GoogleDistanceMatrixApiKey")]
        public string GoogleDistanceMatrixApiKey { get; set; }
        public bool GoogleDistanceMatrixApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }
        public bool IncludeInTopMenu_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.HideInMobileView")]
        public bool HideInMobileView { get; set; }
        public bool HideInMobileView_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.IncludeInFooterColumn")]
        public bool IncludeInFooterColumn { get; set; }
        public bool IncludeInFooterColumn_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.FooterColumnSelector")]
        public string FooterColumnSelector { get; set; }
        public bool FooterColumnSelector_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.PublicDispalyPageSize")]
        public int PublicDispalyPageSize { get; set; }
        public bool PublicDispalyPageSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.Configuration.Fields.PictureSize")]
        public int PictureSize { get; set; }
        public bool PictureSize_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
