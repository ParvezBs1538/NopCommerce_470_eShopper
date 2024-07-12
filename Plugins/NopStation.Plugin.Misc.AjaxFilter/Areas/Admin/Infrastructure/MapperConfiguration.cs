using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<AjaxFilterSettings, ConfigurationModel>()
                    .ForMember(model => model.EnableFilter_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore());

            CreateMap<ConfigurationModel, AjaxFilterSettings>();

            CreateMap<AjaxFilterParentCategoryModel, AjaxFilterParentCategory>()
                .ForMember(model => model.CategoryId, options => options.Ignore());
        }
    }
}
