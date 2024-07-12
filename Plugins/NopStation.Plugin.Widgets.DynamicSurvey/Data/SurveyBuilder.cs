using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Data
{
    public class SurveyBuilder : NopEntityBuilder<Survey>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Survey.Name)).AsString(1024).NotNullable()
                .WithColumn(nameof(Survey.Description)).AsString(int.MaxValue).Nullable();
        }
    }
}
