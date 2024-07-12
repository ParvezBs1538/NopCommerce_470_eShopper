using FluentMigrator;
using Nop.Core;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data.Migrations.Upgrade_To_450201
{
    [NopMigration("2024/04/19 10:47:58:1688252", "NopStation.DMS schma update to 4.70.1.0", MigrationProcessType.Update)]
    public class DataMigration46011 : Migration
    {
        public static string TableName<T>() where T : BaseEntity
        {
            return NameCompatibilityManager.GetTableName(typeof(T));
        }

        public override void Down()
        {
        }

        public override void Up()
        {

            if (!Schema.Table(TableName<ShipperDevice>()).Column(nameof(ShipperDevice.LocationUpdatedOnUtc)).Exists())
            {
                Alter.Table(TableName<ShipperDevice>())
                    .AddColumn(nameof(ShipperDevice.LocationUpdatedOnUtc)).AsDateTime().Nullable();
            }
            if (!Schema.Table(TableName<ShipperDevice>()).Column(nameof(ShipperDevice.Online)).Exists())
            {
                Alter.Table(TableName<ShipperDevice>())
                    .AddColumn(nameof(ShipperDevice.Online)).AsBoolean().Nullable().WithDefaultValue(false);
            }
            if (!Schema.Table(TableName<ShipperDevice>()).Column(nameof(ShipperDevice.Latitude)).Exists())
            {
                Alter.Table(TableName<ShipperDevice>())
                    .AddColumn(nameof(ShipperDevice.Latitude)).AsDecimal().NotNullable().WithDefaultValue(0);
            }

            if (!Schema.Table(TableName<ShipperDevice>()).Column(nameof(ShipperDevice.Longitude)).Exists())
            {
                Alter.Table(TableName<ShipperDevice>())
                    .AddColumn(nameof(ShipperDevice.Longitude)).AsDecimal().NotNullable().WithDefaultValue(0);
            }
        }
    }
}
