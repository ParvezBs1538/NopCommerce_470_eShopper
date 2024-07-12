using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.AlgoliaSearch.Domains;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Data
{
    public class AlgoliaUpdatableItemTableBuilder : NopEntityBuilder<AlgoliaUpdatableItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AlgoliaUpdatableItem.EntityName)).AsString(400).Nullable()
                .WithColumn(nameof(AlgoliaUpdatableItem.UpdatedOnUtc)).AsString(400).Nullable();
        }
    }
}