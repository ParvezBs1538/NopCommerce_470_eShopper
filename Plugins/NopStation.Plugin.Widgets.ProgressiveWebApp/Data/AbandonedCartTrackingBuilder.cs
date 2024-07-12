using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Data
{
    public class AbandonedCartTrackingBuilder : NopEntityBuilder<AbandonedCartTracking>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AbandonedCartTracking.CustomerId))
                .AsInt32().ForeignKey<Customer>()
                .WithColumn(nameof(AbandonedCartTracking.LastModifiedOnUtc))
                .AsDateTime()
                .WithColumn(nameof(AbandonedCartTracking.IsQueued))
                .AsBoolean();
        }
    }
}
