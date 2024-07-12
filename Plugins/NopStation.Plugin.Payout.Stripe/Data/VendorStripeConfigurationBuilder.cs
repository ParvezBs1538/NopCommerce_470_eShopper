using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payout.Stripe.Domain;

namespace NopStation.Plugin.Payout.Stripe.Data
{
    public class VendorStripeConfigurationBuilder : NopEntityBuilder<VendorStripeConfiguration>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(VendorStripeConfiguration.VendorId)).AsInt32().NotNullable().ForeignKey<Vendor>().OnDelete(Rule.Cascade);
        }
    }
}
