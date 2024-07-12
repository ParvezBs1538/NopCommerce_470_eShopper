using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Affiliates;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Widgets.AffiliateStation.Data
{
    public class AffiliateCustomerRecordBuilder : NopEntityBuilder<AffiliateCustomer>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AffiliateCustomer.AffiliateId)).AsInt32().ForeignKey<Affiliate>().PrimaryKey()
                .WithColumn(nameof(AffiliateCustomer.CustomerId)).AsInt32().ForeignKey<Customer>().PrimaryKey();
        }
    }
}