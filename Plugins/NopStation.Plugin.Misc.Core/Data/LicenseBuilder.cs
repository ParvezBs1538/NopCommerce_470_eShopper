using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.Core.Domains;

namespace NopStation.Plugin.Misc.Core.Data;

public class LicenseBuilder : NopEntityBuilder<License>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.
            WithColumn(nameof(License.Key)).AsString(int.MaxValue);
    }
}
