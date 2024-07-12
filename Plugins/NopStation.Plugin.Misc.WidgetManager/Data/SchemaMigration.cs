using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Misc.WidgetManager.Data;

[NopMigration("2022/02/26 09:40:55:1687906", "NopStation.WidgetManager base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<WidgetZoneMapping>();
        Create.TableFor<MonthlyScheduleMapping>();
        Create.TableFor<WeeklyScheduleMapping>();
        Create.TableFor<CustomerConditionMapping>();
        Create.TableFor<ProductConditionMapping>();
        Create.TableFor<ConditionGroupMapping>();
        Create.TableFor<ConditionRecord>();
    }
}
