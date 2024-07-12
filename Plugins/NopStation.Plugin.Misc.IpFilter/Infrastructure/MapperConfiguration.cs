using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.IpFilter.Domain;
using NopStation.Plugin.Misc.IpFilter.Models;

namespace NopStation.Plugin.Misc.IpFilter.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<IpBlockRule, IpBlockRuleModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());
            CreateMap<IpBlockRuleModel, IpBlockRule>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            CreateMap<IpRangeBlockRule, IpRangeBlockRuleModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());
            CreateMap<IpRangeBlockRuleModel, IpRangeBlockRule>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            CreateMap<CountryBlockRule, CountryBlockRuleModel>()
                .ForMember(model => model.CountryName, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore());
            CreateMap<CountryBlockRuleModel, CountryBlockRule>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());
        }
    }
}
