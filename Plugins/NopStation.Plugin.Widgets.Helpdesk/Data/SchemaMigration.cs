using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Data
{
    [NopMigration("2020/09/15 08:40:55:1687541", "NopStation.Helpdesk schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<HelpdeskDepartment>();
            Create.TableFor<HelpdeskStaff>();
            Create.TableFor<Ticket>();
            Create.TableFor<TicketResponse>();
        }
    }
}
