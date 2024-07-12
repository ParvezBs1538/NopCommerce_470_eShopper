using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Infrastructure;

namespace NopStation.Plugin.Misc.QuoteCart.Data;

[NopMigration("2023/02/20 09:40:55:1687800", "NopStation.Plugin.Misc.QuoteCart update schema", MigrationProcessType.NoMatter)]
public class QuoteFormUpdateSchemaMigration : Migration
{
    public override void Up()
    {
        if (!this.ColumnExists<QuoteRequest>(x => x.AttributeXml))
            Create.Column(nameof(QuoteRequest.AttributeXml)).OnTable(NameCompatibilityManager.GetTableName(typeof(QuoteRequest)))
                .AsString().WithDefaultValue("");

        if (!this.ColumnExists<QuoteRequest>(x => x.GuestEmail))
            Create.Column(nameof(QuoteRequest.GuestEmail)).OnTable(NameCompatibilityManager.GetTableName(typeof(QuoteRequest)))
                .AsString().Nullable();

        if (!this.ColumnExists<QuoteRequest>(x => x.Deleted))
            Create.Column(nameof(QuoteRequest.Deleted)).OnTable(NameCompatibilityManager.GetTableName(typeof(QuoteRequest)))
                .AsBoolean().WithDefaultValue(false);

        if (!this.ColumnExists<QuoteForm>(x => x.Deleted))
            Create.Column(nameof(QuoteForm.Deleted)).OnTable(NameCompatibilityManager.GetTableName(typeof(QuoteForm)))
                .AsBoolean().WithDefaultValue(false);

        if (!this.ColumnExists<QuoteForm>(x => x.SubjectToAcl))
            Create.Column(nameof(QuoteForm.SubjectToAcl)).OnTable(NameCompatibilityManager.GetTableName(typeof(QuoteForm)))
                .AsBoolean().WithDefaultValue(false);

        if (!this.TableExists<FormAttribute>())
        {
            Create.TableFor<FormAttribute>();
        }

        if (!this.TableExists<FormAttributeValue>())
        {
            Create.TableFor<FormAttributeValue>();
        }

        if (!this.TableExists<PredefinedFormAttributeValue>())
        {
            Create.TableFor<PredefinedFormAttributeValue>();
        }

        if (!this.TableExists<FormAttributeMapping>())
        {
            Create.TableFor<FormAttributeMapping>();
        }

        if (!this.TableExists<FormSubmissionAttribute>())
        {
            Create.TableFor<FormSubmissionAttribute>();
        }
    }

    public override void Down()
    {
        if (this.TableExists<FormAttribute>())
        {
            Delete.TableFor<FormAttribute>();
        }

        if (this.TableExists<FormAttributeValue>())
        {
            Delete.TableFor<FormAttributeValue>();
        }

        if (this.TableExists<PredefinedFormAttributeValue>())
        {
            Delete.TableFor<PredefinedFormAttributeValue>();
        }

        if (this.TableExists<FormAttributeMapping>())
        {
            Delete.TableFor<FormAttributeMapping>();
        }

        if (this.TableExists<FormSubmissionAttribute>())
        {
            Delete.TableFor<FormSubmissionAttribute>();
        }

        if (this.ColumnExists<QuoteForm>(x => x.Deleted))
            Delete.Column(nameof(QuoteForm.Deleted)).FromTable(NameCompatibilityManager.GetTableName(typeof(QuoteForm)));

        if (this.ColumnExists<QuoteForm>(x => x.SubjectToAcl))
            Delete.Column(nameof(QuoteForm.SubjectToAcl)).FromTable(NameCompatibilityManager.GetTableName(typeof(QuoteForm)));

        if (this.ColumnExists<QuoteRequest>(x => x.AttributeXml))
            Delete.Column(nameof(QuoteRequest.AttributeXml)).FromTable(NameCompatibilityManager.GetTableName(typeof(QuoteRequest)));

        if (this.ColumnExists<QuoteRequest>(x => x.GuestEmail))
            Delete.Column(nameof(QuoteRequest.GuestEmail)).FromTable(NameCompatibilityManager.GetTableName(typeof(QuoteRequest)));

        if (this.ColumnExists<QuoteRequest>(x => x.Deleted))
            Delete.Column(nameof(QuoteRequest.Deleted)).FromTable(NameCompatibilityManager.GetTableName(typeof(QuoteRequest)));
    }
}
