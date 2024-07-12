using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.EnableJwtSecurity")]
    public bool EnableJwtSecurity { get; set; }
    public bool EnableJwtSecurity_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.SecretKey")]
    public string SecretKey { get; set; }
    public bool SecretKey_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.TokenKey")]
    public string TokenKey { get; set; }
    public bool TokenKey_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.TokenSecret")]
    public string TokenSecret { get; set; }
    public bool TokenSecret_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.CheckIat")]
    public bool CheckIat { get; set; }
    public bool CheckIat_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.TokenSecondsValid")]
    public int TokenSecondsValid { get; set; }
    public bool TokenSecondsValid_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.AndroidVersion")]
    public string AndroidVersion { get; set; }
    public bool AndroidVersion_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.AndriodForceUpdate")]
    public bool AndriodForceUpdate { get; set; }
    public bool AndriodForceUpdate_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.PlayStoreUrl")]
    public string PlayStoreUrl { get; set; }
    public bool PlayStoreUrl_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.IOSVersion")]
    public string IOSVersion { get; set; }
    public bool IOSVersion_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.IOSForceUpdate")]
    public bool IOSForceUpdate { get; set; }
    public bool IOSForceUpdate_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.AppStoreUrl")]
    public string AppStoreUrl { get; set; }
    public bool AppStoreUrl_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.LogoId")]
    [UIHint("Picture")]
    public int LogoId { get; set; }
    public bool LogoId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.LogoSize")]
    public int LogoSize { get; set; }
    public bool LogoSize_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdminApi.Configuration.Fields.ShowChangeBaseUrlPanel")]
    public bool ShowChangeBaseUrlPanel { get; set; }
    public bool ShowChangeBaseUrlPanel_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}