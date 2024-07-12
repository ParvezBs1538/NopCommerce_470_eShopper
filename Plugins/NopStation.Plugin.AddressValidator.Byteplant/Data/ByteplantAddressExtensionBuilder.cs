using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.AddressValidator.Byteplant.Domains;
using Nop.Data.Extensions;

namespace NopStation.Plugin.AddressValidator.Byteplant.Data
{
    public class ByteplantAddressExtensionBuilder : NopEntityBuilder<ByteplantAddressExtension>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(ByteplantAddressExtension.AddressId)).AsInt32().ForeignKey<Address>().PrimaryKey();
        }
    }
}