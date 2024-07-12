using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.StoreLocator
{
    public class StoreLocatorSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string GoogleMapApiKey { get; set; }

        public bool SortPickupPointsByDistance { get; set; }

        public int DistanceCalculationMethodId { get; set; }

        public string GoogleDistanceMatrixApiKey { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public bool HideInMobileView { get; set; }

        public bool IncludeInFooterColumn { get; set; }

        public string FooterColumnSelector { get; set; }

        public int PublicDispalyPageSize { get; set; }

        public int PictureSize { get; set; }
    }
}
