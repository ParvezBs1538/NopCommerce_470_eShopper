using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.AddressValidator.Google.Domains;
using Nop.Data.Extensions;

namespace NopStation.Plugin.AddressValidator.Google.Data
{
    public class GoogleAddressExtensionBuilder : NopEntityBuilder<GoogleAddressExtension>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(GoogleAddressExtension.AddressId)).AsInt32().ForeignKey<Address>().PrimaryKey();
        }
    }
}