using FluentMigrator;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.Core.Domains;

namespace NopStation.Plugin.Misc.Core.Data.UpgradeTo46013;

[NopMigration("2023/05/15 11:00:00", "NopStation.Core change license key length update", MigrationProcessType.Update)]
public class LicenseKeyMigration : MigrationBase
{
    public override void Up()
    {
        var licenseTableName = "NS_License";

        if (Schema.Table(licenseTableName).Column(nameof(License.Key)).Exists())
        {
            //update length
            Alter.Table(licenseTableName).AlterColumn(nameof(License.Key)).AsString(int.MaxValue);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}
