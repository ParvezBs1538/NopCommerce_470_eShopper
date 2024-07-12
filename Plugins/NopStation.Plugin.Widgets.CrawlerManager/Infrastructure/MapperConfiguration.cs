using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.CrawlerManager.Domain;
using NopStation.Plugin.Widgets.CrawlerManager.Models;

namespace NopStation.Plugin.Widgets.CrawlerManager.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<Crawler, CrawlerModel>().ReverseMap();
        }

        public int Order => 1;
    }
}