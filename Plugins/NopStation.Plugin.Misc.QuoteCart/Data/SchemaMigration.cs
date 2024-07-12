using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Data;

[NopMigration("2022/10/13 09:40:55:1687789", "NopStation.Plugin.Misc.QuoteCart base schema", MigrationProcessType.NoMatter)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<QuoteCartItem>();
        Create.TableFor<QuoteForm>();
        Create.TableFor<QuoteRequest>();
        Create.TableFor<QuoteRequestItem>();
        Create.TableFor<QuoteRequestMessage>();
        Create.TableFor<QuoteRequestWhitelist>();
    }
}
