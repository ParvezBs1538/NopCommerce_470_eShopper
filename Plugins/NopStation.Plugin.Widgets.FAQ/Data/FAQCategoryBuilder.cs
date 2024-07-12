using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Data
{
    public class FAQCategoryBuilder : NopEntityBuilder<FAQCategory>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FAQCategory.Name)).AsString(int.MaxValue).NotNullable()
                .WithColumn(nameof(FAQCategory.Description)).AsString(int.MaxValue).NotNullable();
        }
    }
}
