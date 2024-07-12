using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Data
{
    public class FAQItemBuilder : NopEntityBuilder<FAQItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FAQItem.Question)).AsString(int.MaxValue).NotNullable()
                .WithColumn(nameof(FAQItem.Answer)).AsString(int.MaxValue).NotNullable();
        }
    }
}
