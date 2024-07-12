using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<FAQCategory, FAQCategoryModel>()
                    .ForMember(model => model.AvailableStores, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.Locales, options => options.Ignore())
                    .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
            CreateMap<FAQCategoryModel, FAQCategory>()
                    .ForMember(entity => entity.LimitedToStores, options => options.Ignore());

            CreateMap<FAQItem, FAQItemModel>()
                    .ForMember(model => model.AvailableStores, options => options.Ignore())
                    .ForMember(model => model.SelectedCategoryIds, options => options.Ignore())
                    .ForMember(model => model.AvailableFAQCategories, options => options.Ignore())
                    .ForMember(model => model.FAQTags, options => options.Ignore())
                    .ForMember(model => model.InitialFAQTags, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.Locales, options => options.Ignore())
                    .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
            CreateMap<FAQItemModel, FAQItem>()
                    .ForMember(entity => entity.LimitedToStores, options => options.Ignore());
        }
    }
}
