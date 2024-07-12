using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Tax.TaxJar.Models;
using NopStation.Plugin.Tax.TaxJar.Services;

namespace NopStation.Plugin.Tax.TaxJar.Controllers
{
    public class TaxJarController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ITaxJarCategoryService _taxJarCategoryService;
        private readonly IStoreContext _storeContext;
        private readonly ICountryService _countryService;
        private readonly TaxJarManager _taxJarManager;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        #endregion

        #region Ctor

        public TaxJarController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            ITaxJarCategoryService taxJarCategoryService,
            IStoreContext storeContext,
            ICountryService countryService,
            TaxJarManager taxJarManager,
            IStaticCacheManager staticCacheManager,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _taxJarCategoryService = taxJarCategoryService;
            _storeContext = storeContext;
            _countryService = countryService;
            _taxJarManager = taxJarManager;
            _staticCacheManager = staticCacheManager;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Utitlities

        protected async Task<IList<SelectListItem>> GetAllCountriesAsync()
        {
            var selectListItems = (await _countryService.GetAllCountriesAsync())
                .Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();

            selectListItems.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });

            return selectListItems;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(TaxJarPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var taxJarSettings = await _settingService.LoadSettingAsync<TaxJarSettings>(storeId);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeId,
                Token = taxJarSettings.Token,
                CountryId = taxJarSettings.CountryId,
                City = taxJarSettings.City,
                Street = taxJarSettings.Street,
                Zip = taxJarSettings.Zip,
                UseSandBox = taxJarSettings.UseSandBox,
                AppliedOnCheckOutOnly = taxJarSettings.AppliedOnCheckOutOnly,
                DisableItemWiseTax = taxJarSettings.DisableItemWiseTax,
                StateId = taxJarSettings.StateId,
                DefaultTaxCategoryId = taxJarSettings.DefaultTaxCategoryId,
                DisableTaxSubmit = taxJarSettings.DisableTaxSubmit,
                TaxJarApiVersionId = taxJarSettings.TaxJarApiVersionId
            };

            foreach (var taxJarApiVersion in TaxJarDefaults.TaxJarApiVersions.OrderByDescending(x => x.Key))
            {
                model.AvailableTaxJarApiVersions.Add(new SelectListItem
                {
                    Text = taxJarApiVersion.Value,
                    Value = taxJarApiVersion.Key.ToString()
                });
            }

            var categories = await _taxJarCategoryService.GetTaxJarFormattedCategoriesAsync();
            model.AvailableCategories.Add(new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.TaxJar.SelectCategories"),
                Value = "0"
            });
            foreach (var category in categories)
            {
                model.AvailableCategories.Add(new SelectListItem
                {
                    Text = category.Name,
                    Value = category.TaxCategoryId.ToString()
                });
            }
            await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.AvailableStates, model.CountryId);
            model.AvailableCountries = await GetAllCountriesAsync();

            if (storeId > 0)
            {
                model.Token_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.Token, storeId);
                model.CountryId_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.CountryId, storeId);
                model.City_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.City, storeId);
                model.Street_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.Street, storeId);
                model.Zip_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.Zip, storeId);
                model.UseSandBox_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.UseSandBox, storeId);
                model.DisableItemWiseTax_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.DisableItemWiseTax, storeId);
                model.AppliedOnCheckOutOnly_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.AppliedOnCheckOutOnly, storeId);
                model.StateId_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.StateId, storeId);
                model.DefaultTaxCategoryId_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.DefaultTaxCategoryId, storeId);
                model.DisableTaxSubmit_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.DisableTaxSubmit, storeId);
                model.TaxJarApiVersionId_OverrideForStore = await _settingService.SettingExistsAsync(taxJarSettings, x => x.TaxJarApiVersionId, storeId);
            }

            return View("~/Plugins/NopStation.Plugin.Tax.TaxJar/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(TaxJarPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var taxJarSettings = await _settingService.LoadSettingAsync<TaxJarSettings>(storeScope);

            //save settings
            taxJarSettings.Token = model.Token;
            taxJarSettings.CountryId = model.CountryId;
            taxJarSettings.City = model.City;
            taxJarSettings.Street = model.Street;
            taxJarSettings.Zip = model.Zip;
            taxJarSettings.UseSandBox = model.UseSandBox;
            taxJarSettings.DisableItemWiseTax = model.DisableItemWiseTax;
            taxJarSettings.AppliedOnCheckOutOnly = model.AppliedOnCheckOutOnly;
            taxJarSettings.StateId = model.StateId;
            taxJarSettings.DefaultTaxCategoryId = model.DefaultTaxCategoryId;
            taxJarSettings.DisableTaxSubmit = model.DisableTaxSubmit;
            taxJarSettings.TaxJarApiVersionId = model.TaxJarApiVersionId;

            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.Token, model.Token_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.CountryId, model.CountryId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.City, model.City_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.Street, model.Street_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.Zip, model.Zip_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.UseSandBox, model.UseSandBox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.AppliedOnCheckOutOnly, model.AppliedOnCheckOutOnly_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.DisableItemWiseTax, model.DisableItemWiseTax_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.StateId, model.StateId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.DefaultTaxCategoryId, model.DefaultTaxCategoryId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.DisableTaxSubmit, model.DisableTaxSubmit_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxJarSettings, x => x.TaxJarApiVersionId, model.TaxJarApiVersionId_OverrideForStore, storeScope, false);


            //now clear settings cache
            await _settingService.ClearCacheAsync();
            await _staticCacheManager.RemoveByPrefixAsync(TaxJarDefaults.TaxJarCacheKeyPrefix);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> UpdateCategories()
        {
            if (!await _permissionService.AuthorizeAsync(TaxJarPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var taxJarSettings = await _settingService.LoadSettingAsync<TaxJarSettings>(storeScope);

            await _taxJarManager.SyncTaxJarCategoriesAsync(taxJarSettings.Token, taxJarSettings.UseSandBox);
            await _staticCacheManager.RemoveByPrefixAsync(TaxJarDefaults.TaxJarCacheKeyPrefix);

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
