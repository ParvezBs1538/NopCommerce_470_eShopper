using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Data
{
    public class CategoryCategorySEOTemplateMappingBuilder : NopEntityBuilder<CategoryCategorySEOTemplateMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CategoryCategorySEOTemplateMapping.CategoryId)).AsInt32().ForeignKey<Category>()
                .WithColumn(nameof(CategoryCategorySEOTemplateMapping.CategorySEOTemplateId)).AsInt32().ForeignKey<CategorySEOTemplate>();

        }
    }
}
