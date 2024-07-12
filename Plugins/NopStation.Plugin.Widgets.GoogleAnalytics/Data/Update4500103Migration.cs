using System.Collections.Generic;
using FluentMigrator;
using Nop.Core.Domain.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace NopStation.Plugin.Widgets.GoogleAnalytics.Data
{
    [NopMigration("2022/09/14 05:40:55:1237339", "Update 4.50.1.3 NopStation.GoogleAnalytics", MigrationProcessType.Update)]
    public class Update4500103Migration : AutoReversingMigration
    {
        public override void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //do not use DI, because it produces exception on the installation process
            var settingRepository = EngineContext.Current.Resolve<IRepository<Setting>>();
            var settingService = EngineContext.Current.Resolve<ISettingService>();

            settingRepository
                .DeleteAsync(setting => setting.Name == "nopstationgoogleanalyticssettings.savelog").Wait();

            settingRepository
                .InsertAsync(new Setting("nopstationgoogleanalyticssettings.savelog", "False")).Wait();

            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Admin.NopStation.Plugin.Widgets.GoogleAnalytics.SaveLog"] = "Save Log",
                ["Admin.NopStation.Plugin.Widgets.GoogleAnalytics.SaveLog.Hint"] = "Save the request log to show in system log"
            }).Wait();

            settingService.ClearCacheAsync().Wait();
        }
    }
}