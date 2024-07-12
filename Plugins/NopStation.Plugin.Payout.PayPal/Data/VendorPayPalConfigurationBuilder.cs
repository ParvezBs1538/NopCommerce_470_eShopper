using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payout.PayPal.Domain;

namespace NopStation.Plugin.Payout.PayPal.Data
{
    public class VendorPayPalConfigurationBuilder : NopEntityBuilder<VendorPayPalConfiguration>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(VendorPayPalConfiguration.VendorId)).AsInt32().NotNullable().ForeignKey<Vendor>().OnDelete(Rule.Cascade);
        }
    }
}
