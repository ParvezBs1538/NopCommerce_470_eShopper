using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.SMS.Messente.Areas.Admin.Models;
using NopStation.Plugin.SMS.Messente.Domains;

namespace NopStation.Plugin.SMS.Messente.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region SMS template

            CreateMap<SmsTemplate, SmsTemplateModel>()
                .ForMember(model => model.AllowedTokens, options => options.Ignore());
            CreateMap<SmsTemplateModel, SmsTemplate>()
                .ForMember(entity => entity.Name, options => options.Ignore());

            #endregion

            #region Queued sms

            CreateMap<QueuedSms, QueuedSmsModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.SentOn, options => options.Ignore());
            CreateMap<QueuedSmsModel, QueuedSms>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.SentOnUtc, options => options.Ignore());

            #endregion 

            #region Configuration

            CreateMap<MessenteSmsSettings, ConfigurationModel>()
                .ForMember(model => model.EnablePlugin_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SendTestSmsTo, options => options.Ignore())
                .ForMember(model => model.SenderName_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableLog_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PhoneNumberRegex_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Password_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Username_OverrideForStore, options => options.Ignore());
            CreateMap<ConfigurationModel, MessenteSmsSettings>();

            #endregion 
        }

        public int Order => 0;
    }
}
