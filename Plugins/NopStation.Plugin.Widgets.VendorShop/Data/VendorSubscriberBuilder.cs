using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Data
{
    public class VendorSubscriberBuilder : NopEntityBuilder<VendorSubscriber>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(VendorSubscriber.VendorId))
                .AsInt32()
                .ForeignKey<Vendor>()
                .OnDelete(System.Data.Rule.Cascade)
                .WithColumn(nameof(VendorSubscriber.CustomerId))
                .AsInt32()
                .ForeignKey<Customer>()
                .OnDelete(System.Data.Rule.Cascade)
                .WithColumn(nameof(VendorSubscriber.SubscribedOn))
                .AsDateTime()
                .Nullable()
                .WithColumn(nameof(VendorSubscriber.StoreId))
                .AsInt32()
                .ForeignKey<Store>()
                .OnDelete(System.Data.Rule.None);
        }
    }
}