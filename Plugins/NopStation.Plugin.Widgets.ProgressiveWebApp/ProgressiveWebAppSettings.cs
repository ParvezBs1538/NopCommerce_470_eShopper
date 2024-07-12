using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp
{
    public class ProgressiveWebAppSettings : ISettings
    {
        public string VapidSubjectEmail { get; set; }

        public string VapidPublicKey { get; set; }

        public string VapidPrivateKey { get; set; }


        public int AbandonedCartCheckingOffset { get; set; }

        public int UnitTypeId { get; set; }

        public bool DisableSilent { get; set; }

        public string Vibration { get; set; }

        public string SoundFileUrl { get; set; }

        public int DefaultIconId { get; set; }


        public string ManifestName { get; set; }

        public string ManifestShortName { get; set; }

        public string ManifestThemeColor { get; set; }

        public string ManifestBackgroundColor { get; set; }

        public string ManifestDisplay { get; set; }

        public string ManifestStartUrl { get; set; }

        public string ManifestAppScope { get; set; }

        public int ManifestPictureId { get; set; }

    }
}
