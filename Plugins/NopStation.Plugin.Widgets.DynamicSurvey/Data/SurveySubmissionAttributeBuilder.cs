using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Data
{
    public class SurveySubmissionAttributeBuilder : NopEntityBuilder<SurveySubmissionAttribute>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SurveySubmissionAttribute.SurveySubmissionId)).AsInt32().NotNullable().ForeignKey<SurveySubmission>();
        }
    }
}
