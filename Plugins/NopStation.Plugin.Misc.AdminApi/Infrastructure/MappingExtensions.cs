using System;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Misc.AdminApi.Models.Customers;

namespace NopStation.Plugin.Misc.AdminApi.Infrastructure;

public static class MappingExtensions
{
    private static TDestination Map<TDestination>(this object source)
    {
        return AutoMapperConfiguration.Mapper.Map<TDestination>(source);
    }

    public static AdminLoginModel ToAdminLoginModel(this LoginModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return model.Map<AdminLoginModel>();
    }
}