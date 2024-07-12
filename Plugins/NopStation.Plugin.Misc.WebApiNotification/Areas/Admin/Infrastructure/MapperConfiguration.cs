using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Push notf template

            CreateMap<WebApiNotificationTemplate, WebApiNotificationTemplateModel>()
                .ForMember(model => model.AllowedTokens, options => options.Ignore());
            CreateMap<WebApiNotificationTemplateModel, WebApiNotificationTemplate>()
                .ForMember(entity => entity.Name, options => options.Ignore());

            #endregion

            #region Queued notification

            CreateMap<WebApiQueuedNotification, WebApiQueuedNotificationModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.SentOn, options => options.Ignore());
            CreateMap<WebApiQueuedNotificationModel, WebApiQueuedNotification>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.SentOnUtc, options => options.Ignore());

            #endregion 

            #region Configuration

            CreateMap<WebApiNotificationSettings, ConfigurationModel>()
                .ForMember(model => model.GoogleConsoleApiAccessKey_OverrideForStore, options => options.Ignore());
            CreateMap<ConfigurationModel, WebApiNotificationSettings>();

            #endregion

            #region Push notification campaign

            CreateMap<WebApiNotificationCampaign, WebApiNotificationCampaignModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.AddedToQueueOn, options => options.Ignore())
                .ForMember(model => model.AvailableSmartGroups, options => options.Ignore())
                .ForMember(model => model.CustomerRoles, options => options.Ignore())
                .ForMember(model => model.DeviceTypes, options => options.Ignore())
                .ForMember(model => model.AvailableCustomerRoles, options => options.Ignore())
                .ForMember(model => model.SendingWillStartOn, options => options.Ignore());
            CreateMap<WebApiNotificationCampaignModel, WebApiNotificationCampaign>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CustomerRoles, options => options.Ignore())
                .ForMember(entity => entity.DeviceTypes, options => options.Ignore())
                .ForMember(entity => entity.AddedToQueueOnUtc, options => options.Ignore())
                .ForMember(entity => entity.SendingWillStartOnUtc, options => options.Ignore());

            #endregion
        }

        public int Order => 0;
    }
}
