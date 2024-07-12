using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Directory;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Data
{
    public class RedxAreaBuilder : NopEntityBuilder<RedxArea>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(RedxArea.RedxAreaId)).AsInt32().PrimaryKey()
                .WithColumn(nameof(RedxArea.StateProvinceId)).AsInt32().Nullable().ForeignKey<StateProvince>();
        }
    }
}