using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.EmailValidator.Abstract.Domains;
using NopStation.Plugin.EmailValidator.Abstract.Models;

namespace NopStation.Plugin.EmailValidator.Abstract.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Abstract emails

            CreateMap<AbstractEmail, AbstractEmailModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.ValidationCount, options => options.Ignore());
            CreateMap<AbstractEmailModel, AbstractEmail>()
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            #endregion

            #region Configuration

            CreateMap<AbstractEmailValidatorSettings, ConfigurationModel>()
                .ForMember(model => model.EnablePlugin_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.BlockedDomains_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableLog_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RevalidateInvalidEmailsAfterHours_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ApiKey_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ValidateCustomerAddressEmail_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ValidateCustomerInfoEmail_OverrideForStore, options => options.Ignore());
            CreateMap<ConfigurationModel, AbstractEmailValidatorSettings>()
                .ForMember(setting => setting.BlockedDomains, options => options.Ignore());

            #endregion 
        }

        public int Order => 0;
    }
}
