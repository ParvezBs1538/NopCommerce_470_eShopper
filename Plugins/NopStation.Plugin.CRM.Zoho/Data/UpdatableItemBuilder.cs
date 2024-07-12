using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.CRM.Zoho.Domain;

namespace NopStation.Plugin.CRM.Zoho.Data
{
    public class UpdatableItemBuilder : NopEntityBuilder<UpdatableItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(UpdatableItem.EntityId)).AsInt32().PrimaryKey()
                .WithColumn(nameof(UpdatableItem.EntityTypeId)).AsInt32().PrimaryKey();
        }
    }
}