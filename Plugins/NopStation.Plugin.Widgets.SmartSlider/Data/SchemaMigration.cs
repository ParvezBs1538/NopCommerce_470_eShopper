using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Data;

[NopMigration("2022/07/03 12:24:16:2552019", "NopStation.SmartSlider base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<SmartSlider>();
        Create.TableFor<SmartSliderItem>();
    }
}
