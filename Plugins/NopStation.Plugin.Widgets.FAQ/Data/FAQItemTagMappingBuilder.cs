using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Data
{
    public class FAQItemTagMappingBuilder : NopEntityBuilder<FAQItemTag>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FAQItemTag.FAQItemId)).AsInt32().ForeignKey<FAQItem>().PrimaryKey()
                .WithColumn(nameof(FAQItemTag.FAQTagId)).AsInt32().ForeignKey<FAQTag>().PrimaryKey();
        }
    }
}
