using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.EmailValidator.Verifalia.Domains;

namespace NopStation.Plugin.EmailValidator.Verifalia.Data
{
    public class VerifaliaEmailBuilder : NopEntityBuilder<VerifaliaEmail>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(VerifaliaEmail.Email)).AsString(1000).NotNullable();
        }
    }
}