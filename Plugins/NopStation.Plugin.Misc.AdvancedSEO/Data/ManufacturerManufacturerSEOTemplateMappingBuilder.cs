using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Data
{
    public class ManufacturerManufacturerSEOTemplateMappingBuilder : NopEntityBuilder<ManufacturerManufacturerSEOTemplateMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ManufacturerManufacturerSEOTemplateMapping.ManufacturerId)).AsInt32().ForeignKey<Manufacturer>()
                .WithColumn(nameof(ManufacturerManufacturerSEOTemplateMapping.ManufacturerSEOTemplateId)).AsInt32().ForeignKey<ManufacturerSEOTemplate>();

        }
    }
}
