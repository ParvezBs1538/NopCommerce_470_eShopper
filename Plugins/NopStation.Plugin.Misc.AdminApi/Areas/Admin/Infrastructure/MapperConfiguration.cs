using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    #region Ctor

    public MapperConfiguration()
    {
        CreateMap<AdminApiSettings, ConfigurationModel>()
            .ForMember(model => model.CheckIat_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.EnableJwtSecurity_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.TokenKey_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.TokenSecondsValid_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.TokenSecret_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.AndriodForceUpdate_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.AndroidVersion_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.PlayStoreUrl_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.IOSForceUpdate_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.IOSVersion_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.AppStoreUrl_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.LogoId_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.LogoSize_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowChangeBaseUrlPanel_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, AdminApiSettings>();
    }

    #endregion

    #region Properties

    public int Order => 0;

    #endregion
}