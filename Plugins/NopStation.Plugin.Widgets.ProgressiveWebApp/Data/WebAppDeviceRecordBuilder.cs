using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Data
{
    public class WebAppDeviceRecordBuilder : NopEntityBuilder<WebAppDevice>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WebAppDevice.PushEndpoint))
                .AsString(450)
                .WithColumn(nameof(WebAppDevice.PushP256DH))
                .AsString(int.MaxValue)
                .WithColumn(nameof(WebAppDevice.PushAuth))
                .AsString()
                .WithColumn(nameof(WebAppDevice.CustomerId))
                .AsInt32()
                .WithColumn(nameof(WebAppDevice.StoreId))
                .AsInt32()
                .WithColumn(nameof(WebAppDevice.VapidPublicKey))
                .AsString(int.MaxValue)
                .WithColumn(nameof(WebAppDevice.VapidPrivateKey))
                .AsString(int.MaxValue)
                .WithColumn(nameof(WebAppDevice.CreatedOnUtc))
                .AsDateTime();
        }
    }
}