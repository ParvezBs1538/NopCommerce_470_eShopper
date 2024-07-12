using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Data
{
    [NopMigration("2023/11/06 08:37:55:1687545", "NopStation.DynamicSurvey base scheme", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<Survey>();
            Create.TableFor<SurveyAttribute>();
            Create.TableFor<SurveyAttributeMapping>();
            Create.TableFor<SurveyAttributeValue>();
            Create.TableFor<SurveySubmission>();
            Create.TableFor<SurveySubmissionAttribute>();
            Create.TableFor<PredefinedSurveyAttributeValue>();
        }
    }
}
