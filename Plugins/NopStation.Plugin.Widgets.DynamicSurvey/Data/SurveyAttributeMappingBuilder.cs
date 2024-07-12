using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Data
{
    public class SurveyAttributeMappingBuilder : NopEntityBuilder<SurveyAttributeMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SurveyAttributeMapping.SurveyAttributeId)).AsInt32().NotNullable().ForeignKey<SurveyAttribute>()
                .WithColumn(nameof(SurveyAttributeMapping.SurveyId)).AsInt32().NotNullable().ForeignKey<Survey>();
        }
    }
}
