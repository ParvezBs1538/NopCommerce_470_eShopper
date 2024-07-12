using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.PopupLogin.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.PopupLogin.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<PopupLoginSettings, ConfigurationModel>()
                .ForMember(model => model.EnablePopupLogin_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.LoginUrlElementSelector_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, PopupLoginSettings>();
        }
    }
}
