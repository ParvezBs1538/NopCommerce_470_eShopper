using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.AdminApi;

public class AdminApiSettings : ISettings
{
    public bool EnableJwtSecurity { get; set; }

    public string SecretKey { get; set; }

    public string TokenKey { get; set; }

    public string TokenSecret { get; set; }

    public bool CheckIat { get; set; }

    public int TokenSecondsValid { get; set; }

    public string AndroidVersion { get; set; }

    public bool AndriodForceUpdate { get; set; }

    public string PlayStoreUrl { get; set; }

    public string IOSVersion { get; set; }

    public bool IOSForceUpdate { get; set; }

    public string AppStoreUrl { get; set; }

    public int LogoId { get; set; }

    public int LogoSize { get; set; }

    public bool ShowChangeBaseUrlPanel { get; set; }
}
