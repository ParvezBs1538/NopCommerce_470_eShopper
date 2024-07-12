using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.Product360View.Domain;
using NopStation.Plugin.Widgets.Product360View.Models;

namespace NopStation.Plugin.Widgets.Product360View.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<ProductImageSetting360, ImageSetting360Model>().ReverseMap();
            CreateMap<ProductPictureMapping360, ProductPicture360Model>().ReverseMap();
        }

        public int Order => 1;
    }
}
