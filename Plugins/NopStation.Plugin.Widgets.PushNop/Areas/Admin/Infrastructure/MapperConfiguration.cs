using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region SmartGroup

            CreateMap<SmartGroup, SmartGroupModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());
            CreateMap<SmartGroupModel, SmartGroup>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            #endregion

            #region Smart group notification

            CreateMap<SmartGroupNotification, GroupNotificationModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.AddedToQueueOn, options => options.Ignore())
                .ForMember(model => model.SmartGroupName, options => options.Ignore())
                .ForMember(model => model.Subscriptions, options => options.Ignore())
                .ForMember(model => model.AvailableSmartGroups, options => options.Ignore())
                .ForMember(model => model.SendingWillStartOn, options => options.Ignore());
            CreateMap<GroupNotificationModel, SmartGroupNotification>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.AddedToQueueOnUtc, options => options.Ignore())
                .ForMember(entity => entity.SendingWillStartOnUtc, options => options.Ignore());

            #endregion

            #region Smart group condition

            CreateMap<SmartGroupCondition, SmartGroupConditionModel>()
                .ForMember(model => model.AvailableConditionColumnTypes, options => options.Ignore())
                .ForMember(model => model.AvailableConditionTypes, options => options.Ignore())
                .ForMember(model => model.AvailableLogicTypes, options => options.Ignore())
                .ForMember(model => model.SmartGroupName, options => options.Ignore());
            CreateMap<SmartGroupConditionModel, SmartGroupCondition>()
                .ForMember(entity => entity.LogicType, options => options.Ignore())
                .ForMember(entity => entity.ConditionColumnType, options => options.Ignore())
                .ForMember(entity => entity.ConditionType, options => options.Ignore());

            #endregion
        }

        public int Order => 0;
    }
}
