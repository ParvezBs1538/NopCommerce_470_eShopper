using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Data
{
    public partial class PredefinedSurveyAttributeValueBuilder : NopEntityBuilder<PredefinedSurveyAttributeValue>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PredefinedSurveyAttributeValue.Name)).AsString(400).NotNullable()
                .WithColumn(nameof(PredefinedSurveyAttributeValue.SurveyAttributeId)).AsInt32().ForeignKey<SurveyAttribute>();
        }

        #endregion
    }
}