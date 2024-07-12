using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Data
{
    [NopMigration("2021/07/14 08:40:55:1687542", "NopStation.FAQ base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<FAQCategory>();
            Create.TableFor<FAQTag>();
            Create.TableFor<FAQItem>();
            Create.TableFor<FAQItemCategory>();
            Create.TableFor<FAQItemTag>();
        }
    }
}
