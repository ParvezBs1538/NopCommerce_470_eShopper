using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.CRM.Zoho.Domain;

namespace NopStation.Plugin.CRM.Zoho.Data
{
    public class ZohoDataMappingBuilder : NopEntityBuilder<DataMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DataMapping.EntityId)).AsInt32().PrimaryKey()
                .WithColumn(nameof(DataMapping.EntityTypeId)).AsInt32().PrimaryKey();
        }
    }
}