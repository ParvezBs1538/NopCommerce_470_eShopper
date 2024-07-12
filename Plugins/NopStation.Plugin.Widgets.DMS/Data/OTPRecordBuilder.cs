using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data
{
    public class OTPRecordBuilder : NopEntityBuilder<OTPRecord>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(OTPRecord.CustomerId)).AsInt32().ForeignKey<Customer>()
                .WithColumn(nameof(OTPRecord.OrderId)).AsInt32().ForeignKey<Order>()
                //.WithColumn(nameof(OTPRecord.PhoneNumber)).AsString().Nullable()
                .WithColumn(nameof(OTPRecord.VerifiedByShipperId)).AsInt32().Nullable()
                .WithColumn(nameof(OTPRecord.VerifiedOnUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(OTPRecord.ShipmentId)).AsInt32().ForeignKey<Shipment>(onDelete: Rule.None);
        }
    }
}
