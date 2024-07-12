using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Data
{
    public class SurveyAttributeValueBuilder : NopEntityBuilder<SurveyAttributeValue>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SurveyAttributeValue.SurveyAttributeMappingId)).AsInt32().ForeignKey<SurveyAttributeMapping>()
                .WithColumn(nameof(SurveyAttributeValue.Name)).AsString(int.MaxValue).NotNullable();
        }
    }
}
