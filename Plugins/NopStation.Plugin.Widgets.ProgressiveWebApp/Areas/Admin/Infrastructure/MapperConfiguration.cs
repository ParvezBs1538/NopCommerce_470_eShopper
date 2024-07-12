using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Web app device

            #endregion

            #region Push notf template

            CreateMap<PushNotificationTemplate, PushNotificationTemplateModel>()
                .ForMember(model => model.AllowedTokens, options => options.Ignore());
            CreateMap<PushNotificationTemplateModel, PushNotificationTemplate>()
                .ForMember(entity => entity.Name, options => options.Ignore());

            #endregion

            #region Push notf announce

            CreateMap<PushNotificationAnnouncement, PushNotificationAnnouncementModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());
            CreateMap<PushNotificationAnnouncementModel, PushNotificationAnnouncement>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            #endregion

            #region Device

            CreateMap<WebAppDevice, WebAppDeviceModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.StoreName, options => options.Ignore())
                .ForMember(model => model.CustomerName, options => options.Ignore());
            CreateMap<WebAppDeviceModel, WebAppDevice>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            #endregion 

            #region Queued notification

            CreateMap<QueuedPushNotification, QueuedPushNotificationModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.SentOn, options => options.Ignore());
            CreateMap<QueuedPushNotificationModel, QueuedPushNotification>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.SentOnUtc, options => options.Ignore());

            #endregion 

            #region Configuration

            CreateMap<ProgressiveWebAppSettings, ConfigurationModel>();
            CreateMap<ConfigurationModel, ProgressiveWebAppSettings>();

            #endregion 
        }

        public int Order => 0;
    }
}
