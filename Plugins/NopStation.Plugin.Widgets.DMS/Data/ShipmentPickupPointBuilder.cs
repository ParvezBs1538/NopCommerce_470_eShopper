using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data
{
    public class ShipmentPickupPointBuilder : NopEntityBuilder<ShipmentPickupPoint>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ShipmentPickupPoint.AddressId)).AsInt32().ForeignKey<Address>().OnDelete(Rule.None);
        }
    }
}
