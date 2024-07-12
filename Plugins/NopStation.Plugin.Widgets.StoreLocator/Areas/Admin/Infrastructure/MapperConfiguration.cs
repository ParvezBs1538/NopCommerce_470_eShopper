using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models;
using NopStation.Plugin.Widgets.StoreLocator.Domain;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<StoreLocatorSettings, ConfigurationModel>()
                    .ForMember(model => model.DistanceCalculationMethodId_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.EnablePlugin_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.FooterColumnSelector_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.GoogleDistanceMatrixApiKey_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.GoogleMapApiKey_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.HideInMobileView_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.IncludeInFooterColumn_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.IncludeInTopMenu_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.PictureSize_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.PublicDispalyPageSize_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.SortPickupPointsByDistance_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, StoreLocatorSettings>();

            CreateMap<StoreLocation, StoreLocationModel>()
                    .ForMember(model => model.Locales, options => options.Ignore())
                    .ForMember(model => model.PictureUrl, options => options.Ignore())
                    .ForMember(model => model.Address, options => options.Ignore());
            CreateMap<StoreLocationModel, StoreLocation>()
                    .ForMember(entity => entity.AddressId, options => options.Ignore())
                    .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                    .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<StoreLocationPicture, StoreLocationPictureModel>()
                .ForMember(model => model.OverrideAltAttribute, options => options.Ignore())
                .ForMember(model => model.OverrideTitleAttribute, options => options.Ignore())
                .ForMember(model => model.PictureUrl, options => options.Ignore());
        }
    }
}
