using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Tax.TaxJar.Domains;

namespace NopStation.Plugin.Tax.TaxJar.Infrastructure.Migrations.UpgradeTo4_50_1_1
{
    [NopMigration("2022-08-01 00:00:00", "Update Migration for NopStation Taxjar 4.50.1.1", MigrationProcessType.Update)]
    public class DataMigration : Migration
    {

        private readonly INopDataProvider _dataProvider;

        public DataMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override async void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            var taxjarTransactionLogTableName = "NS_TaxJar_TransactionLog";

            if (!Schema.Table(taxjarTransactionLogTableName).Exists())
            {
                Create.TableFor<TaxjarTransactionLog>();
            }

            var orderTaxAmountColumn = "OrderTaxAmount";

            if (!Schema.Table(taxjarTransactionLogTableName).Column(orderTaxAmountColumn).Exists())
            {
                Alter.Table(taxjarTransactionLogTableName)
                    .AddColumn(orderTaxAmountColumn).AsDecimal(18, 8).NotNullable().SetExistingRowsTo(0);
            }


            //do not use DI, because it produces exception on the installation process
            var settingService = EngineContext.Current.Resolve<ISettingService>();

            var taxJarSettings = settingService.LoadSettingAsync<TaxJarSettings>().Result;

            bool settingUpdated = false;


            if (!settingService.SettingExistsAsync(taxJarSettings, settings => settings.AppliedOnCheckOutOnly).Result)
            {
                var appliedOnCartOnlySettingName = "TaxJarSettings.AppliedOnCartOnly";
                var appliedOnCartOnlySetting = settingService.GetSettingByKeyAsync(appliedOnCartOnlySettingName, true, loadSharedValueIfNotFound: true);
                taxJarSettings.AppliedOnCheckOutOnly = appliedOnCartOnlySetting.Result;

                //do not use DI, because it produces exception on the installation process
                var settingRepository = EngineContext.Current.Resolve<IRepository<Setting>>();

                //miniprofiler settings are moved to appSettings
                settingRepository
                    .DeleteAsync(setting => setting.Name == appliedOnCartOnlySettingName).Wait();

                settingUpdated = true;
            }

            if (!settingService.SettingExistsAsync(taxJarSettings, settings => settings.TaxJarApiVersionId).Result)
            {
                taxJarSettings.TaxJarApiVersionId = TaxJarDefaults.TaxJarApiVersions.OrderByDescending(x => x.Key).FirstOrDefault().Key;
                settingUpdated = true;
            }

            if (!settingService.SettingExistsAsync(taxJarSettings, settings => settings.DisableTaxSubmit).Result)
            {
                taxJarSettings.DisableTaxSubmit = false;
                settingUpdated = true;
            }

            if (!settingService.SettingExistsAsync(taxJarSettings, settings => settings.DisableItemWiseTax).Result)
            {
                taxJarSettings.DisableItemWiseTax = true;
                settingUpdated = true;
            }

            if (!settingService.SettingExistsAsync(taxJarSettings, settings => settings.TaxRateCacheTime).Result)
            {
                taxJarSettings.TaxRateCacheTime = TaxJarDefaults.TaxRateCacheTime;
                settingUpdated = true;
            }

            if (settingUpdated)
                settingService.SaveSettingAsync(taxJarSettings).Wait();

            var resourceList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.TaxJarApiVersion", "Api version"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.TaxJarApiVersion.Hint", "TaxJar has introduced API versioning to deliver enhanced validations and features."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.ConditionForDefultCategory", "4. Select Defult category for use as general tax category."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.DisableTaxSubmit", "Disable tax submit"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.DisableTaxSubmit.Hint", "Disable tax submit on taxjar site"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.TransactionId", "Transaction id"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.TransactionId.Hint", "Transaction id/ Order guid."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.TransactionReferance", "Transaction referance"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.TransactionReferance.Hint", "Transaction referance if refund processed."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.TransactionType", "Transaction type"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.TransactionType.Hint", "Transaction type."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.User", "User"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.User.Hint", "User id"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.Amount", "Amount"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.Amount.Hint", "Amount"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.Customer.Hint", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.TransactionDate", "Transaction date"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.TransactionDate.Hint", "Transaction date."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.Hint", "View log entry details"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.BackToList", "back to log"),
                new KeyValuePair<string, string>("admin.nopstation.taxjar.log", "Transaction log"),
                new KeyValuePair<string, string>("Plugins.NopStation.TaxJar.Log.Search.CreatedTo", "Created To"),
                new KeyValuePair<string, string>("Plugins.NopStation.TaxJar.Log.Search.CreatedFrom", "Created From"),
                new KeyValuePair<string, string>("Plugins.NopStation.TaxJar.Log.Search.CreatedTo.Hint", "The creation to date for the search."),
                new KeyValuePair<string, string>("Plugins.NopStation.TaxJar.Log.Search.CreatedFrom.Hint", "The creation from date for the search."),

                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.TaxRateCacheTime", "Tax rate cache time in minutes"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.TaxRateCacheTime.Hint", "Taxjax calculated tax caching time in minute(s). Minimum time is 1 minute and maximum time is 5 minutes"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.AppliedOnCheckOutOnly.Hint", "Calculate tax only checkout page"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.AppliedOnCheckOutOnly", "Apply tax on checkout only"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.Order", "Nop Order Id"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.OrderId", "Nop Order Id"),

            };

            var localizationService = NopInstance.Load<ILocalizationService>();
            foreach (var resource in resourceList)
                await localizationService.AddOrUpdateLocaleResourceAsync(resource.Key, resource.Value);

        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
