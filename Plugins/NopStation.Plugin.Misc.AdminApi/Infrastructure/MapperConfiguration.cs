using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Misc.AdminApi.Models.Customers;

namespace NopStation.Plugin.Misc.AdminApi.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    #region Ctor

    public MapperConfiguration()
    {
        CreateMap<AdminLoginModel, LoginModel>();
        CreateMap<LoginModel, AdminLoginModel>()
            .ForMember(model => model.LanguageNavSelector, options => options.Ignore());
    }

    #endregion

    #region Properties

    public int Order => 0;

    #endregion
}