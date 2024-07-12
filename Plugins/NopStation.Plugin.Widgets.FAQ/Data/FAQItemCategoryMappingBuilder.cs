using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Data
{
    public class FAQItemCategoryMappingBuilder : NopEntityBuilder<FAQItemCategory>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FAQItemCategory.FAQItemId)).AsInt32().ForeignKey<FAQItem>().PrimaryKey()
                .WithColumn(nameof(FAQItemCategory.FAQCategoryId)).AsInt32().ForeignKey<FAQCategory>().PrimaryKey();
        }
    }
}
