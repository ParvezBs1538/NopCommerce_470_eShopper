using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.AddressValidator.EasyPost.Domains;
using Nop.Data.Extensions;

namespace NopStation.Plugin.AddressValidator.EasyPost.Data
{
    public class EasyPostAddressExtensionBuilder : NopEntityBuilder<EasyPostAddressExtension>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(EasyPostAddressExtension.AddressId)).AsInt32().ForeignKey<Address>().PrimaryKey();
        }
    }
}