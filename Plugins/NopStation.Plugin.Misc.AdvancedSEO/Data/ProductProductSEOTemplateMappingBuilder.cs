using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Data
{
    public class ProductProductSEOTemplateMappingBuilder : NopEntityBuilder<ProductProductSEOTemplateMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ProductProductSEOTemplateMapping.ProductId)).AsInt32().ForeignKey<Product>()
                .WithColumn(nameof(ProductProductSEOTemplateMapping.ProductSEOTemplateId)).AsInt32().ForeignKey<ProductSEOTemplate>();
        }
    }
}
