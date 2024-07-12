using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Core.Domain.Vendors;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Shipping.VendorAndState.Domain;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Directory;

namespace NopStation.Plugin.Shipping.VendorAndState.Data
{
    public class VendorStateProvinceShippingBuilder : NopEntityBuilder<VendorStateProvinceShipping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
               .WithColumn(nameof(VendorStateProvinceShipping.VendorId)).AsInt32().ForeignKey<Vendor>()
               .WithColumn(nameof(VendorStateProvinceShipping.ShippingMethodId)).AsInt32().ForeignKey<ShippingMethod>()
               .WithColumn(nameof(VendorStateProvinceShipping.StateProvinceId)).AsInt32().ForeignKey<StateProvince>();
        }
    }
}
