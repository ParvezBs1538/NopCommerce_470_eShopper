using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Controllers
{
    public class WebApiSliderController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISliderModelFactory _sliderModelFactory;
        private readonly IApiSliderService _sliderService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly ITopicService _topicService;

        #endregion

        #region Ctor

        public WebApiSliderController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISliderModelFactory sliderModelFactory,
            IApiSliderService sliderService,
            IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService,
            ICategoryService categoryService,
            IProductService productService,
            IManufacturerService manufacturerService,
            IVendorService vendorService,
            ITopicService topicService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _sliderModelFactory = sliderModelFactory;
            _sliderService = sliderService;
            _dateTimeHelper = dateTimeHelper;
            _permissionService = permissionService;
            _categoryService = categoryService;
            _productService = productService;
            _manufacturerService = manufacturerService;
            _vendorService = vendorService;
            _topicService = topicService;
        }

        #endregion

        #region Utilities

        public async Task ValidateSlider(SliderModel model)
        {
            if (model.SliderTypeId == (int)SliderType.Category)
            {
                var category = await _categoryService.GetCategoryByIdAsync(model.EntityId);
                if (category == null || category.Deleted)
                    ModelState.AddModelError("EntityId", await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidCategory"));
            }
            else if (model.SliderTypeId == (int)SliderType.Manufacturer)
            {
                var manufacterer = await _manufacturerService.GetManufacturerByIdAsync(model.EntityId);
                if (manufacterer == null || manufacterer.Deleted)
                    ModelState.AddModelError("EntityId", await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidManufacturer"));
            }
            else if (model.SliderTypeId == (int)SliderType.Product)
            {
                var product = await _productService.GetProductByIdAsync(model.EntityId);
                if (product == null || product.Deleted)
                    ModelState.AddModelError("EntityId", await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidProduct"));
            }
            else if (model.SliderTypeId == (int)SliderType.Vendor)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(model.EntityId);
                if (vendor == null || vendor.Deleted)
                    ModelState.AddModelError("EntityId", await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidVendor"));
            }
            else if (model.SliderTypeId == (int)SliderType.Topic)
            {
                var topic = await _topicService.GetTopicByIdAsync(model.EntityId);
                if (topic == null)
                    ModelState.AddModelError("EntityId", await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidTopic"));
            }
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
                return AccessDeniedView();

            var searchModel = await _sliderModelFactory.PrepareSliderSearchModelAsync(new SliderSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> GetList(SliderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
                return await AccessDeniedDataTablesJson();

            var model = await _sliderModelFactory.PrepareSliderListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
                return AccessDeniedView();

            var model = await _sliderModelFactory.PrepareSliderModelAsync(new SliderModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(SliderModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
                return AccessDeniedView();

            await ValidateSlider(model);

            if (ModelState.IsValid)
            {
                var slider = model.ToEntity<ApiSlider>();

                if (model.ActiveEndDate.HasValue)
                    slider.ActiveEndDateUtc = _dateTimeHelper.ConvertToUtcTime(model.ActiveEndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
                if (model.ActiveStartDate.HasValue)
                    slider.ActiveStartDateUtc = _dateTimeHelper.ConvertToUtcTime(model.ActiveStartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
                slider.CreatedOnUtc = DateTime.UtcNow;
                await _sliderService.InsertApiSliderAsync(slider);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = slider.Id });
            }

            model = await _sliderModelFactory.PrepareSliderModelAsync(model, null);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
                return AccessDeniedView();

            var slider = await _sliderService.GetApiSliderByIdAsync(id);
            if (slider == null)
                return RedirectToAction("List");

            var model = await _sliderModelFactory.PrepareSliderModelAsync(null, slider);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(SliderModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
                return AccessDeniedView();

            await ValidateSlider(model);

            var slider = await _sliderService.GetApiSliderByIdAsync(model.Id);
            if (slider == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                slider = model.ToEntity(slider);

                if (model.ActiveEndDate.HasValue)
                    slider.ActiveEndDateUtc = _dateTimeHelper.ConvertToUtcTime(model.ActiveEndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
                if (model.ActiveStartDate.HasValue)
                    slider.ActiveStartDateUtc = _dateTimeHelper.ConvertToUtcTime(model.ActiveStartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

                await _sliderService.UpdateApiSliderAsync(slider);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = slider.Id });
            }

            model = await _sliderModelFactory.PrepareSliderModelAsync(model, slider);

            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
                return AccessDeniedView();

            var slider = await _sliderService.GetApiSliderByIdAsync(id);
            if (slider == null)
                return RedirectToAction("List");

            await _sliderService.DeleteApiSliderAsync(slider);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Deleted"));

            return RedirectToAction("List");
        }

        [EditAccessAjax]
        public async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
                return AccessDeniedView();

            if (selectedIds != null)
                await _sliderService.DeleteApiSlidersAsync(_sliderService.GetApiSliderByIds(selectedIds.ToArray()).ToList());

            return Json(new { Result = true });
        }

        #endregion
    }
}
