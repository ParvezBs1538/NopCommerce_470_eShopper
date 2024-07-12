using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Data.Migrations;

[NopMigration("2023-08-17 12:00:00", "NopStation.Plugin.Misc.QuoteCart update schema", MigrationProcessType.Update)]
public class RentalInfoMigration : MigrationBase
{
    public override void Up()
    {
        var quoteRequestItemTableName = NameCompatibilityManager.GetTableName(typeof(QuoteRequestItem));
        var quoteCartItemTableName = NameCompatibilityManager.GetTableName(typeof(QuoteCartItem));

        if (!Schema.Table(quoteRequestItemTableName).Column(nameof(QuoteRequestItem.RentalStartDateUtc)).Exists())
            Alter.Table(quoteRequestItemTableName)
                .AddColumn(nameof(QuoteRequestItem.RentalStartDateUtc)).AsDateTime().Nullable();

        if (!Schema.Table(quoteRequestItemTableName).Column(nameof(QuoteRequestItem.RentalEndDateUtc)).Exists())
            Alter.Table(quoteRequestItemTableName)
               .AddColumn(nameof(QuoteRequestItem.RentalEndDateUtc)).AsDateTime().Nullable();

        if (!Schema.Table(quoteCartItemTableName).Column(nameof(QuoteCartItem.RentalStartDateUtc)).Exists())
            Alter.Table(quoteCartItemTableName)
              .AddColumn(nameof(QuoteCartItem.RentalStartDateUtc)).AsDateTime().Nullable();

        if (!Schema.Table(quoteCartItemTableName).Column(nameof(QuoteCartItem.RentalEndDateUtc)).Exists())

            Alter.Table(quoteCartItemTableName)
               .AddColumn(nameof(QuoteCartItem.RentalEndDateUtc)).AsDateTime().Nullable();
    }

    public override void Down()
    {

    }
}
