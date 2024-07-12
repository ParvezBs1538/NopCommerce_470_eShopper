using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data
{
    public class ShipperDeviceBuilder : NopEntityBuilder<ShipperDevice>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ShipperDevice.DeviceToken))
                .AsString().Nullable()
                .WithColumn(nameof(ShipperDevice.DeviceTypeId))
                .AsInt32()
                .WithColumn(nameof(Shipper.CustomerId)).AsInt32().ForeignKey<Customer>()
                .WithColumn(nameof(ShipperDevice.SubscriptionId))
                .AsString().Nullable()
                .WithColumn(nameof(ShipperDevice.CreatedOnUtc))
                .AsDateTime()
                .WithColumn(nameof(ShipperDevice.UpdatedOnUtc))
                .AsDateTime()
                .WithColumn(nameof(ShipperDevice.LocationUpdatedOnUtc))
                .AsDateTime().Nullable()
                .WithColumn(nameof(ShipperDevice.Online))
                .AsBoolean().Nullable().WithDefaultValue(false)
                .WithColumn(nameof(ShipperDevice.Latitude))
                .AsDecimal().NotNullable().WithDefaultValue(0)
                .WithColumn(nameof(ShipperDevice.Longitude))
                .AsDecimal().NotNullable().WithDefaultValue(0);
        }
    }
}
