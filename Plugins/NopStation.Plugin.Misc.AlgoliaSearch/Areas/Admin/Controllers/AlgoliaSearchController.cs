using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Extensions;
using NopStation.Plugin.Misc.AlgoliaSearch.Factories;
using NopStation.Plugin.Misc.AlgoliaSearch.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Controllers
{
    public class AlgoliaSearchController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IAlgoliaSearchModelFactory _algoliaSearchModelFactory;
        private readonly IAlgoliaHelperFactory _helperFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IAlgoliaUpdatableItemService _algoliaUpdatableItemService;
        private readonly ICustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public AlgoliaSearchController(INotificationService notificationService,
            IAlgoliaSearchModelFactory algoliaSearchModelFactory,
            IAlgoliaHelperFactory helperFactory,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPermissionService permissionService,
            IAlgoliaUpdatableItemService algoliaUpdatableItemService,
            ICustomerActivityService customerActivityService)
        {
            _notificationService = notificationService;
            _algoliaSearchModelFactory = algoliaSearchModelFactory;
            _helperFactory = helperFactory;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _permissionService = permissionService;
            _algoliaUpdatableItemService = algoliaUpdatableItemService;
            _customerActivityService = customerActivityService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _algoliaSearchModelFactory.PrepareConfigurationModelAsync();
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var algoliaSearchSettings = await _settingService.LoadSettingAsync<AlgoliaSearchSettings>(storeScope);
            algoliaSearchSettings = model.ToSettings(algoliaSearchSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AdminKey, model.AdminKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowAttributeFilter, model.AllowAttributeFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowCategoryFilter, model.AllowCategoryFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowCustomersToSelectPageSize, model.AllowCustomersToSelectPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowedSortingOptions, model.AllowedSortingOptions_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowManufacturerFilter, model.AllowManufacturerFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowPriceRangeFilter, model.AllowPriceRangeFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowProductSorting, model.AllowProductSorting_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowProductViewModeChanging, model.AllowProductViewModeChanging_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowRatingFilter, model.AllowRatingFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowSpecificationFilter, model.AllowSpecificationFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AllowVendorFilter, model.AllowVendorFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.ApplicationId, model.ApplicationId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.AutoCompleteListSize, model.AutoCompleteListSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.DefaultViewMode, model.DefaultViewMode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.EnableAutoComplete, model.EnableAutoComplete_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.HidePoweredByAlgolia, model.HidePoweredByAlgolia_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.MaximumAttributesShowInFilter, model.MaximumAttributesShowInFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.MaximumCategoriesShowInFilter, model.MaximumCategoriesShowInFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.MaximumManufacturersShowInFilter, model.MaximumManufacturersShowInFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.MaximumVendorsShowInFilter, model.MaximumSpecificationsShowInFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.MaximumVendorsShowInFilter, model.MaximumVendorsShowInFilter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.MonitoringKey, model.MonitoringKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.SearchOnlyKey, model.SearchOnlyKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.SearchPagePageSizeOptions, model.SearchPagePageSizeOptions_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.SearchTermMinimumLength, model.SearchPageProductsPerPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.SearchTermMinimumLength, model.SearchTermMinimumLength_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.ShowProductImagesInSearchAutoComplete, model.ShowProductImagesInSearchAutoComplete_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.ShowProductsCount, model.ShowProductsCount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.UsageKey, model.UsageKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(algoliaSearchSettings, x => x.EnableMultilingualSearch, model.EnableMultilingualSearch_OverrideForStore, storeScope, false);
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [HttpPost]
        public virtual async Task<IActionResult> ValueDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageUploadProducts))
                return AccessDeniedView();

            var algoliaUpdatableItem = await _algoliaUpdatableItemService.GetAlgoliaUpdatableItemByIdAsync(id)
                ?? throw new ArgumentException("No updatable item found with the specified id", nameof(id));

            await _algoliaUpdatableItemService.DeleteAlgoliaUpdatableItemAsync(algoliaUpdatableItem);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteUpdatableItem", $"Updatable item delete for specified id-{algoliaUpdatableItem.Id}",
                 algoliaUpdatableItem);

            return new NullJsonResult();
        }
        public async Task<IActionResult> UpdatableItem()
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageUploadProducts))
                return AccessDeniedView();

            return View(new UpdatableItemSearchModel());
        }

        [HttpPost]
        public async Task<IActionResult> UpdatableItemList(UpdatableItemSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageUploadProducts))
                return await AccessDeniedDataTablesJson();

            var data = await _algoliaSearchModelFactory.PrepareUpdatableItemListModelAsync(searchModel);
            return Json(data);
        }

        [HttpPost, ActionName("UpdatableItem"), EditAccess]
        public async Task<IActionResult> UpdateAllItems()
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageUploadProducts))
                return AccessDeniedView();

            await _helperFactory.UpdateAlgoliaItemAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.UploadProduct.UpdatedAllItems"));
            return RedirectToAction("UpdatableItem");
        }

        public async Task<IActionResult> UploadProduct()
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageUploadProducts))
                return AccessDeniedView();

            return View(new UploadProductModel());
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> UploadProduct(UploadProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageUploadProducts))
                return await AccessDeniedDataTablesJson();

            await _helperFactory.UploadProductsAsync(model);
            return Json(new { Message = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.UploadProduct.UploadCompleted") });
        }

        [HttpPost, EditAccess]
        public async Task<IActionResult> ClearIndex()
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageUploadProducts))
                return AccessDeniedView();

            _helperFactory.ClearIndex();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.IndexCleared"));
            return RedirectToAction("Configure");
        }

        [HttpPost, EditAccess]
        public async Task<IActionResult> UpdateIndex(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageUploadProducts))
                return AccessDeniedView();

            await _helperFactory.UpdateIndicesAsync(model);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.IndexUpdated"));
            return RedirectToAction("Configure");
        }

        #endregion
    }
}
