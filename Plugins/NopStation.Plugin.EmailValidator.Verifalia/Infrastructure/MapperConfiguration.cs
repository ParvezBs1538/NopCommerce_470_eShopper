using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.EmailValidator.Verifalia.Domains;
using NopStation.Plugin.EmailValidator.Verifalia.Models;

namespace NopStation.Plugin.EmailValidator.Verifalia.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Verifalia emails

            CreateMap<VerifaliaEmail, VerifaliaEmailModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.Status, options => options.Ignore())
                .ForMember(model => model.ValidationCount, options => options.Ignore());
            CreateMap<VerifaliaEmailModel, VerifaliaEmail>()
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.Status, options => options.Ignore());

            #endregion

            #region Configuration

            CreateMap<VerifaliaEmailValidatorSettings, ConfigurationModel>()
                .ForMember(model => model.EnablePlugin_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.BlockedDomains_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableLog_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Password_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.QualityLevel_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RevalidateInvalidEmailsAfterHours_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Username_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ValidateCustomerAddressEmail_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ValidateCustomerInfoEmail_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ValidateQuality_OverrideForStore, options => options.Ignore());
            CreateMap<ConfigurationModel, VerifaliaEmailValidatorSettings>()
                .ForMember(setting => setting.BlockedDomains, options => options.Ignore());

            #endregion 
        }

        public int Order => 0;
    }
}
