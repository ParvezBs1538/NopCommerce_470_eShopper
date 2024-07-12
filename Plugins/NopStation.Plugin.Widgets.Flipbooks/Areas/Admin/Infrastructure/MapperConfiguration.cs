using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Flipbooks.Domains;

namespace NopStation.Plugin.Widgets.Flipbooks.Admin.Areas.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<FlipbookModel, Flipbook>()
                .ForMember(entity => entity.AvailableEndDateTimeUtc, options => options.Ignore())
                .ForMember(entity => entity.AvailableStartDateTimeUtc, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.LimitedToStores, options => options.Ignore());
            CreateMap<Flipbook, FlipbookModel>()
                .ForMember(model => model.AvailableStores, options => options.Ignore())
                .ForMember(model => model.AvailableFlipbooks, options => options.Ignore())
                .ForMember(model => model.AvailableEndDateTime, options => options.Ignore())
                .ForMember(model => model.AvailableStartDateTime, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.SelectedStoreIds, options => options.Ignore());

            CreateMap<FlipbookContentModel, FlipbookContent>();
            CreateMap<FlipbookContent, FlipbookContentModel>()
                .ForMember(model => model.FlipbookName, options => options.Ignore())
                .ForMember(model => model.ImageUrl, options => options.Ignore());

            CreateMap<FlipbookContentProductModel, FlipbookContentProduct>();
            CreateMap<FlipbookContentProduct, FlipbookContentProductModel>()
                .ForMember(model => model.ProductName, options => options.Ignore());
        }
    }
}
