using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Tax.TaxJar.Services;

namespace NopStation.Plugin.Tax.TaxJar
{
    public class TaxJarProvider : BasePlugin, ITaxProvider, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ITaxJarCategoryService _taxJarCategoryService;
        private readonly ITaxJarService _taxJarService;
        private readonly TaxJarSettings _taxJarSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TaxJarManager _taxJarManager;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public TaxJarProvider(ILocalizationService localizationService,
            ISettingService settingService,
            IPermissionService permissionService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ITaxJarCategoryService taxJarCategoryService,
            ITaxJarService taxJarService,
            TaxJarSettings taxJarSettings,
            TaxJarManager taxJarManager,
            IWorkContext workContext,
            IStaticCacheManager staticCacheManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _permissionService = permissionService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _taxJarCategoryService = taxJarCategoryService;
            _taxJarService = taxJarService;
            _taxJarSettings = taxJarSettings;
            _taxJarManager = taxJarManager;
            _workContext = workContext;
            _staticCacheManager = staticCacheManager;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Methods

        public async Task<TaxRateResult> GetTaxRateAsync(TaxRateRequest taxRateRequest)
        {
            if (!await _taxJarService.IsTaxJarPluginConfiguredAsync())
                return new TaxRateResult { Errors = new List<string> { "TaxJar plugin is not configured." } };

            if (taxRateRequest.Address == null)
                return new TaxRateResult { Errors = new List<string> { "Address is not set" } };

            if (_taxJarSettings.DisableItemWiseTax)
                return new TaxRateResult { TaxRate = 0 };

            var product = taxRateRequest.Product ?? new Product();
            if (product == null)
                return new TaxRateResult { TaxRate = 0 };

            var taxJarCategory = await _taxJarCategoryService.GetTaxJarCategoryByTaxCategoryIdAsync(product.TaxCategoryId);
            if (taxJarCategory == null)
                taxJarCategory = await _taxJarCategoryService.GetTaxJarCategoryByTaxCategoryIdAsync(_taxJarSettings.DefaultTaxCategoryId);

            //prepare cache key
            var customer = taxRateRequest.Customer ?? await _workContext.GetCurrentCustomerAsync();
            var taxCategoryId = taxRateRequest.TaxCategoryId > 0
                ? taxRateRequest.TaxCategoryId
                : taxRateRequest.Product?.TaxCategoryId ?? 0;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(TaxJarDefaults.TaxRateCacheKey,
                customer,
                taxCategoryId,
                taxRateRequest.Address.Address1,
                taxRateRequest.Address.City,
                taxRateRequest.Address.StateProvinceId ?? 0,
                taxRateRequest.Address.CountryId ?? 0,
                taxRateRequest.Address.ZipPostalCode);

            cacheKey.CacheTime = _taxJarSettings.TaxRateCacheTime < 1 || _taxJarSettings.TaxRateCacheTime > TaxJarDefaults.TaxRateCacheTime ? TaxJarDefaults.TaxRateCacheTime : _taxJarSettings.TaxRateCacheTime;

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var lineItems = new List<LineItem>
                {
                    new LineItem
                    {
                        id = product.Id.ToString(),
                        discount = 0,
                        quantity = 1,
                        product_identifier = product.Id.ToString(),
                        product_tax_code = taxJarCategory?.TaxCode,
                        unit_price = taxRateRequest.Price
                    }
                };

                var taxRate = await _taxJarManager.GetTaxRateAsync(taxRateRequest.Address, lineItems,
                    taxRateRequest.Price, product.AdditionalShippingCharge, product.Sku, customer);

                return new TaxRateResult { TaxRate = taxRate?.Rate ?? 0 };
            });
        }

        public async Task<TaxTotalResult> GetTaxTotalAsync(TaxTotalRequest taxTotalRequest)
        {
            if (!await _taxJarService.IsTaxJarPluginConfiguredAsync())
                return new TaxTotalResult { Errors = new List<string> { "TaxJar plugin is not configured." } };

            var controller = _httpContextAccessor.HttpContext.Request.RouteValues["controller"].ToString().ToLower();

            if (_taxJarSettings.AppliedOnCheckOutOnly && !controller.Equals("checkout", StringComparison.InvariantCultureIgnoreCase))
            {
                return new TaxTotalResult();
            }
            else
            {
                //cache tax total within the request
                var key = $"nop.TaxJar.TaxTotal-{taxTotalRequest.UsePaymentMethodAdditionalFee}";
                if (!(_httpContextAccessor.HttpContext.Items.TryGetValue(key, out var result) &&
                    result is TaxTotalResult taxTotalResult))
                {
                    //and get tax details
                    taxTotalResult = await _taxJarManager.CreateTaxTotalTransactionAsync(taxTotalRequest);

                    _httpContextAccessor.HttpContext.Items.TryAdd(key, taxTotalResult);
                }

                return taxTotalResult;
            }
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/TaxJar/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new TaxJarSettings
            {
                TaxJarApiVersionId = TaxJarDefaults.TaxJarApiVersions.OrderByDescending(x => x.Key).FirstOrDefault().Key,
                DisableItemWiseTax = true,
                AppliedOnCheckOutOnly = true,
                DisableTaxSubmit = false,
                UseSandBox = false,
                TaxRateCacheTime = TaxJarDefaults.TaxRateCacheTime,

            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new TaxJarPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<TaxJarSettings>();
            await this.UninstallPluginAsync(new TaxJarPermissionProvider());
            await base.UninstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            if (targetVersion != currentVersion && targetVersion == "4.3.4")
            {
                if (!await _settingService.SettingExistsAsync(_taxJarSettings, x => x.TaxJarApiVersionId))
                {
                    _taxJarSettings.TaxJarApiVersionId = TaxJarDefaults.TaxJarApiVersions.OrderByDescending(x => x.Key).FirstOrDefault().Key;
                    await _settingService.SaveSettingAsync(_taxJarSettings);
                }

                await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.NopStation.TaxJar.Configuration.Fields.TaxJarApiVersion", "Api version");
                await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.NopStation.TaxJar.Configuration.Fields.TaxJarApiVersion.Hint", "TaxJar has introduced API versioning to deliver enhanced validations and features.");
            }
            await base.UpdateAsync(currentVersion, targetVersion);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.TaxJar.Menu.TaxJar"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(TaxJarPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.TaxJar.Menu.Configuration"),
                    Url = "~/Admin/TaxJar/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "TaxJar.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/tax-jar-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=tax-jar",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Menu.TaxJar", "TaxJar"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.ProductTaxCategory.Hint", "Choose a product tax category for transaction."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.ProductTaxCategory", "Product tax category"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.UseSandBox.Hint", "Use as a test area."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.UseSandBox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.AppliedOnCheckOutOnly.Hint", "Calculate tax only checkout page"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.AppliedOnCheckOutOnly", "Apply on checkout only"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.DisableItemWiseTax.Hint", "Disable item wise tax rate calcutaion."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.DisableItemWiseTax", "Disable item wise tax"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.Token.Hint", "Token for connecting api."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.Token", "Api Token"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.Street.Hint", "Street for the TaxJar account."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.Street", "Street"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.City.Hint", "City for the TaxJar account."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.City", "City"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.Zip.Hint", "5 digit zipcode for the TaxJar account."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.Zip", "Zip"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.State.Hint", "Two-letter ISO state code for the TaxJar account."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.State", "State"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.Country.Hint", "Choose a country for the TaxJar account."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.Country", "Country"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.DefaultTaxCategory.Hint", "Choose a tax category as default"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.DefaultTaxCategory", "Default tax category"),
                new KeyValuePair<string, string>("admin.nopstation.TaxJar.Configuration", "Tax Jar settings"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.DisableTaxSubmit", "Disable tax submit"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.DisableTaxSubmit.Hint", "Disable tax submit on taxjar site"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.SelectCategories", "Select one"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Update Categories", "Update categories"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.BackToList", " back to tax provider list"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.ConfirmationText", "It removes previous all tax categories and add new tax categories. Do you want this?"),

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

                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Note", "Note:"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.ConditionForCountry", "1. Country is required."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.ConditionForZip", "2. If country is 'US' then zip is required."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.ConditionForState", "3. If country is 'US' or 'CA' then state is required."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.ConditionForDefultCategory", "4. Select Defult category for use as general tax category."),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.TaxJarApiVersion", "Api version"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.TaxJarApiVersion.Hint", "TaxJar has introduced API versioning to deliver enhanced validations and features."),

                //4.50.1.1
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.TaxRateCacheTime", "Tax rate cache time in minutes"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.TaxRateCacheTime.Hint", "Taxjax calculated tax caching time in minute(s). Minimum time is 1 minute and maximum time is 5 minutes"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.AppliedOnCheckOutOnly.Hint", "Calculate tax only checkout page"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Configuration.Fields.AppliedOnCheckOutOnly", "Apply tax on checkout only"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.Order", "Nop Order Id"),
                new KeyValuePair<string, string>("Admin.NopStation.TaxJar.Log.OrderId", "Nop Order Id"),
            };
            return list;
        }

        #endregion
    }
}
