using System;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Widgets.DMS.Models.Shippers;

namespace NopStation.Plugin.Widgets.DMS.Infrastructure
{
    public static class MappingExtensions
    {
        private static TDestination Map<TDestination>(this object source)
        {
            return AutoMapperConfiguration.Mapper.Map<TDestination>(source);
        }

        public static ShipperLoginModel ToShipperLoginModel(this LoginModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return model.Map<ShipperLoginModel>();
        }
    }
}