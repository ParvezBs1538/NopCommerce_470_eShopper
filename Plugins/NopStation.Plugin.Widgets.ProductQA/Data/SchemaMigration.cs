using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Domains;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Data
{
    [NopMigration("2020/08/16 12:24:16:2551777", "NopPluginNopStation.Plugin.Widgets.ProductQuestionAnswer base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<ProductQnA>();
        }
    }
}
