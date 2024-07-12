using FluentMigrator;
using Nop.Data.Migrations;

namespace NopStation.Plugin.Misc.WebApi.Data.Migrations
{
    [NopMigration("2023-01-20 00:00:00", "NopStation.WebApi CategoryIcon table update", MigrationProcessType.Update)]
    public class UpgradeTo_460_1_1 : Migration
    {
        public UpgradeTo_460_1_1()
        {
        }

        public override void Up()
        {

            //CategoryIcon table
            var categoryIconTableName = "NS_WebApi_CategoryIcon";

            //remove column
            var categoryIconColumnName = "PictureId";

            if (Schema.Table(categoryIconTableName).Column(categoryIconColumnName).Exists())
                Delete.Column(categoryIconColumnName).FromTable(categoryIconTableName);
        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
