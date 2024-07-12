using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Domains;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Updatable item

            CreateMap<AlgoliaUpdatableItem, UpdatableItemModel>()
                .ForMember(model => model.Name, options => options.Ignore())
                .ForMember(model => model.UpdatedByCustomerName, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore());
            CreateMap<UpdatableItemModel, AlgoliaUpdatableItem>();

            #endregion 

            #region Settings

            CreateMap<AlgoliaSearchSettings, ConfigurationModel>()
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore())
                .ForMember(model => model.AvailableSortOptions, options => options.Ignore())
                .ForMember(model => model.UpdateIndicesModel, options => options.Ignore())
                .ForMember(model => model.CanClearOrUpdateIndex, options => options.Ignore());
            CreateMap<ConfigurationModel, AlgoliaSearchSettings>();

            #endregion
        }

        public int Order => 0;
    }
}
