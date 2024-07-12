using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Core;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Controllers
{
    public class DeliverySchedulerController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IDeliverySchedulerModelFactory _deliverySchedulerModelFactory;
        private readonly ISpecialDeliveryOffsetService _specialDeliveryOffsetService;

        #endregion

        #region Ctor

        public DeliverySchedulerController(IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IDeliverySchedulerModelFactory deliverySchedulerModelFactory,
            ISpecialDeliveryOffsetService specialDeliveryOffsetService)
        {
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _deliverySchedulerModelFactory = deliverySchedulerModelFactory;
            _specialDeliveryOffsetService = specialDeliveryOffsetService;
        }

        #endregion

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _deliverySchedulerModelFactory.PrepareConfigurationModelAsync();

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<DeliverySchedulerSettings>(storeScope);
            settings = model.ToSettings(settings);

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableScheduling, model.EnableScheduling_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.NumberOfDaysToDisplay, model.NumberOfDaysToDisplay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DisplayDayOffset, model.DisplayDayOffset_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DateFormat, model.DateFormat_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowRemainingCapacity, model.ShowRemainingCapacity_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [HttpPost]
        public async Task<IActionResult> SpecialOffsetList(CategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _deliverySchedulerModelFactory.PrepareOffsetListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> SpecialOffsetUpdate(SpecialDeliveryOffsetModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var offset = await _specialDeliveryOffsetService.GetSpecialDeliveryOffsetByIdCategoryAsync(model.Id);
            if (offset != null)
            {
                offset.DaysOffset = model.DaysOffset;
                await _specialDeliveryOffsetService.UpdateSpecialDeliveryOffsetAsync(offset);
            }
            else
            {
                offset = new Domains.SpecialDeliveryOffset()
                {
                    CategoryId = model.Id,
                    DaysOffset = model.DaysOffset
                };
                await _specialDeliveryOffsetService.InsertSpecialDeliveryOffsetAsync(offset);
            }

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> SpecialOffsetReset(SpecialDeliveryOffsetModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var offset = await _specialDeliveryOffsetService.GetSpecialDeliveryOffsetByIdCategoryAsync(model.Id);
            if (offset != null)
                await _specialDeliveryOffsetService.DeleteSpecialDeliveryOffsetAsync(offset);

            return new NullJsonResult();
        }
    }
}
