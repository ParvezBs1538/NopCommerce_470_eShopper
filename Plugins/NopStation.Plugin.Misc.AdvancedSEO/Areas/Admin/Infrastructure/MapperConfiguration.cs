using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryCategorySEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategorySEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerManufacturerSEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerSEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductProductSEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductSEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<CategorySEOTemplate, CategorySEOTemplateModel>()
                    .ForMember(model => model.AvailableStores, options => options.Ignore())
                    .ForMember(model => model.CreatedByCustomer, options => options.Ignore())
                    .ForMember(model => model.LastUpdatedByCustomer, options => options.Ignore())
                    .ForMember(model => model.CreatedOn, options => options.Ignore())
                    .ForMember(model => model.UpdatedOn, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
            CreateMap<CategorySEOTemplateModel, CategorySEOTemplate>()
                    .ForMember(entity => entity.LimitedToStores, options => options.Ignore())
                    .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                    .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<ManufacturerSEOTemplate, ManufacturerSEOTemplateModel>()
                    .ForMember(model => model.AvailableStores, options => options.Ignore())
                    .ForMember(model => model.CreatedByCustomer, options => options.Ignore())
                    .ForMember(model => model.LastUpdatedByCustomer, options => options.Ignore())
                    .ForMember(model => model.CreatedOn, options => options.Ignore())
                    .ForMember(model => model.UpdatedOn, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
            CreateMap<ManufacturerSEOTemplateModel, ManufacturerSEOTemplate>()
                    .ForMember(entity => entity.LimitedToStores, options => options.Ignore())
                    .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                    .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<ProductSEOTemplate, ProductSEOTemplateModel>()
                    .ForMember(model => model.AvailableStores, options => options.Ignore())
                    .ForMember(model => model.CreatedByCustomer, options => options.Ignore())
                    .ForMember(model => model.LastUpdatedByCustomer, options => options.Ignore())
                    .ForMember(model => model.CreatedOn, options => options.Ignore())
                    .ForMember(model => model.UpdatedOn, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
            CreateMap<ProductSEOTemplateModel, ProductSEOTemplate>()
                    .ForMember(entity => entity.LimitedToStores, options => options.Ignore())
                    .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                    .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<CategoryCategorySEOTemplateMapping, CategoryCategorySEOTemplateMappingModel>()
                    .ForMember(model => model.CategoryName, options => options.Ignore())
                    .ReverseMap();

            CreateMap<ManufacturerManufacturerSEOTemplateMapping, ManufacturerManufacturerSEOTemplateMappingModel>()
                    .ForMember(model => model.ManufacturerName, options => options.Ignore())
                    .ReverseMap();

            CreateMap<ProductProductSEOTemplateMapping, ProductProductSEOTemplateMappingModel>()
                    .ForMember(model => model.ProductName, options => options.Ignore())
                    .ReverseMap();
        }
    }
}
