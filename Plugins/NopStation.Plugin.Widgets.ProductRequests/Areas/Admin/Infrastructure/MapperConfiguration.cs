using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductRequests.Domains;

namespace NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<ProductRequestSettings, ConfigurationModel>()
                    .ForMember(model => model.AllowedCustomerRolesIds_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.FooterElementSelector_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.IncludeInFooter_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.IncludeInTopMenu_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.DescriptionRequired_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.LinkRequired_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.MinimumProductRequestCreateInterval_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, ProductRequestSettings>();

            CreateMap<ProductRequest, ProductRequestModel>()
                    .ForMember(model => model.CreatedOn, options => options.Ignore())
                    .ForMember(model => model.Customer, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.Store, options => options.Ignore());
            CreateMap<ProductRequestModel, ProductRequest>()
                    .ForMember(model => model.CustomerId, options => options.Ignore())
                    .ForMember(model => model.StoreId, options => options.Ignore())
                    .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());
        }
    }
}
