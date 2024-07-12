using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using Nop.Data.Extensions;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Data
{
    public class SpecialDeliveryOffsetBuilder : NopEntityBuilder<SpecialDeliveryOffset>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SpecialDeliveryOffset.CategoryId)).AsInt32().ForeignKey<Category>().PrimaryKey();
        }
    }
}
