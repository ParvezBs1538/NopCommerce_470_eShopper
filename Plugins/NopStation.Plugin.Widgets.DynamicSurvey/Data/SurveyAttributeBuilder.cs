using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Data
{
    public class SurveyAttributeBuilder : NopEntityBuilder<SurveyAttribute>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SurveyAttribute.Name)).AsString(1024).NotNullable()
                .WithColumn(nameof(SurveyAttribute.Description)).AsString(int.MaxValue).Nullable();
        }
    }
}
