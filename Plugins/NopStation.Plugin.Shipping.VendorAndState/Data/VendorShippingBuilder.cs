using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Core.Domain.Vendors;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Shipping.VendorAndState.Domain;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Shipping.VendorAndState.Data
{
    public class VendorShippingBuilder : NopEntityBuilder<VendorShipping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
               .WithColumn(nameof(VendorShipping.VendorId)).AsInt32().ForeignKey<Vendor>()
               .WithColumn(nameof(VendorShipping.ShippingMethodId)).AsInt32().ForeignKey<ShippingMethod>();
        }
    }
}
