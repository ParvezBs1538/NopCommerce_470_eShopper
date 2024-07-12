using System;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Data;

public class QuoteRequestEntityBuilder : NopEntityBuilder<QuoteRequest>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(QuoteRequest.RequestId))
            .AsGuid()
            .Unique()
            .WithDefaultValue(Guid.Empty);
    }
}
