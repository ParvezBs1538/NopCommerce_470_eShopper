using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerManufacturerSEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerToMap;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerSEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Services.Catalog;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Controllers
{
    public class ManufacturerSEOTemplateController : NopStationAdminController
    {
        private readonly IManufacturerService _manufacturerService;
        #region Fields

        private readonly IManufacturerSEOTemplateModelFactory _manufacturerSEOTemplateModelFactory;
        private readonly IManufacturerSEOTemplateService _manufacturerSEOTemplateService;
        private readonly IManufacturerManufacturerSEOTemplateMappingService _manufacturerManufacturerSEOTemplateMappingService;
        private readonly IManufacturerManufacturerSEOTemplateMappingModelFactory _manufacturerManufacturerSEOTemplateMappingModelFactory;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ManufacturerSEOTemplateController(
            IManufacturerService manufacturerService,
            IManufacturerSEOTemplateModelFactory manufacturerSEOTemplateModelFactory,
            IManufacturerSEOTemplateService manufacturerSEOTemplateService,
            IManufacturerManufacturerSEOTemplateMappingService manufacturerManufacturerSEOTemplateMappingService,
            IManufacturerManufacturerSEOTemplateMappingModelFactory manufacturerManufacturerSEOTemplateMappingModelFactory,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IWorkContext workContext
            )
        {
            _manufacturerService = manufacturerService;
            _manufacturerSEOTemplateModelFactory = manufacturerSEOTemplateModelFactory;
            _manufacturerSEOTemplateService = manufacturerSEOTemplateService;
            _manufacturerManufacturerSEOTemplateMappingService = manufacturerManufacturerSEOTemplateMappingService;
            _manufacturerManufacturerSEOTemplateMappingModelFactory = manufacturerManufacturerSEOTemplateMappingModelFactory;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _workContext = workContext;
        }

        #endregion

        #region Utilites

        protected virtual async Task SaveStoreMappingsAsync(ManufacturerSEOTemplate manufacturerSEOTemplate, ManufacturerSEOTemplateModel model)
        {
            manufacturerSEOTemplate.LimitedToStores = model.SelectedStoreIds.Any();
            await _manufacturerSEOTemplateService.UpdateManufacturerSEOTemplateAsync(manufacturerSEOTemplate);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(manufacturerSEOTemplate);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                        await _storeMappingService.InsertStoreMappingAsync(manufacturerSEOTemplate, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                }
            }
        }

        protected virtual async Task UpdateLocalesAsync(ManufacturerSEOTemplate manufacturerSEOTemplate, ManufacturerSEOTemplateModel model)
        {

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(manufacturerSEOTemplate,
                    x => x.SEOTitleTemplate,
                    localized.SEOTitleTemplate,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(manufacturerSEOTemplate,
                    x => x.SEODescriptionTemplate,
                    localized.SEODescriptionTemplate,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(manufacturerSEOTemplate,
                    x => x.SEOKeywordsTemplate,
                    localized.SEOKeywordsTemplate,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        #region ManufacturerSEOTemplate

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return AccessDeniedView();

            //prepare model
            var model = await _manufacturerSEOTemplateModelFactory.PrepareManufacturerSEOTemplateSearchModelAsync(new ManufacturerSEOTemplateSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ManufacturerSEOTemplateSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _manufacturerSEOTemplateModelFactory.PrepareManufacturerSEOTemplateListModelAsync(searchModel);

            return Json(model);
        }

        #region Create / Edit / Delete

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return AccessDeniedView();

            //prepare model
            var model = await _manufacturerSEOTemplateModelFactory.PrepareManufacturerSEOTemplateModelAsync(new ManufacturerSEOTemplateModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ManufacturerSEOTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var manufacturerSEOTemplate = model.ToEntity<ManufacturerSEOTemplate>();
                manufacturerSEOTemplate.CreatedOnUtc = DateTime.UtcNow;
                manufacturerSEOTemplate.UpdatedOnUtc = DateTime.UtcNow;
                manufacturerSEOTemplate.CreatedByCustomerId = customer.Id;
                manufacturerSEOTemplate.LastUpdatedByCustomerId = customer.Id;
                await _manufacturerSEOTemplateService.InsertManufacturerSEOTemplateAsync(manufacturerSEOTemplate);

                //stores
                await SaveStoreMappingsAsync(manufacturerSEOTemplate, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = manufacturerSEOTemplate.Id });
            }

            //prepare model
            model = await _manufacturerSEOTemplateModelFactory.PrepareManufacturerSEOTemplateModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return AccessDeniedView();

            //try to get a manufacturerSEOTemplate with the specified id
            var manufacturerSEOTemplate = await _manufacturerSEOTemplateService.GetManufacturerSEOTemplateByIdAsync(id);
            if (manufacturerSEOTemplate == null || manufacturerSEOTemplate.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _manufacturerSEOTemplateModelFactory.PrepareManufacturerSEOTemplateModelAsync(new ManufacturerSEOTemplateModel(), manufacturerSEOTemplate);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ManufacturerSEOTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return AccessDeniedView();

            //try to get a manufacturerSEOTemplate with the specified id
            var manufacturerSEOTemplate = await _manufacturerSEOTemplateService.GetManufacturerSEOTemplateByIdAsync(model.Id);
            if (manufacturerSEOTemplate == null || manufacturerSEOTemplate.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                manufacturerSEOTemplate = model.ToEntity(manufacturerSEOTemplate);
                manufacturerSEOTemplate.UpdatedOnUtc = DateTime.UtcNow;
                manufacturerSEOTemplate.LastUpdatedByCustomerId = customer.Id;
                await _manufacturerSEOTemplateService.UpdateManufacturerSEOTemplateAsync(manufacturerSEOTemplate);

                //stores
                await SaveStoreMappingsAsync(manufacturerSEOTemplate, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = manufacturerSEOTemplate.Id });
            }

            //prepare model
            model = await _manufacturerSEOTemplateModelFactory.PrepareManufacturerSEOTemplateModelAsync(model, manufacturerSEOTemplate, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return AccessDeniedView();

            //try to get a manufacturerSEOTemplate with the specified id
            var manufacturerSEOTemplate = await _manufacturerSEOTemplateService.GetManufacturerSEOTemplateByIdAsync(id);
            if (manufacturerSEOTemplate == null)
                return RedirectToAction("List");

            await _manufacturerSEOTemplateService.DeleteManufacturerSEOTemplateAsync(manufacturerSEOTemplate);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Deleted"));

            return RedirectToAction("List");
        }

        #endregion



        #region Mapping

        [HttpPost]
        public virtual async Task<IActionResult> ManufacturerSEOTemplateMappingList(ManufacturerManufacturerSEOTemplateMappingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return await AccessDeniedDataTablesJson();

            var manufacturerSEOTemplate = await _manufacturerSEOTemplateService.GetManufacturerSEOTemplateByIdAsync(searchModel.ManufacturerSEOTemplateId);
            if (manufacturerSEOTemplate == null)
                return BadRequest();

            //prepare model
            var model = await _manufacturerManufacturerSEOTemplateMappingModelFactory.PrepareManufacturerManufacturerSEOTemplateMappingListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<ActionResult> ManufacturerSEOTemplateMappingDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return await AccessDeniedDataTablesJson();

            var manufacturerSEOTemplateMapping = await _manufacturerManufacturerSEOTemplateMappingService.GetManufacturerManufacturerSEOTemplateMappingByIdAsync(id);

            if (manufacturerSEOTemplateMapping != null)
                await _manufacturerManufacturerSEOTemplateMappingService.DeleteManufacturerManufacturerSEOTemplateMappingAsync(manufacturerSEOTemplateMapping);

            return new NullJsonResult();
        }

        #region Add Manufacturer To Mapping

        public virtual async Task<IActionResult> ManufacturerAddPopup(int manufacturerSEOTemplateId)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return AccessDeniedView();


            var manufacturerSEOTemplate = await _manufacturerSEOTemplateService.GetManufacturerSEOTemplateByIdAsync(manufacturerSEOTemplateId);
            if (manufacturerSEOTemplate == null)
                return BadRequest();

            //prepare model
            var model = await _manufacturerManufacturerSEOTemplateMappingModelFactory.PrepareManufacturerToMapSearchModelAsync(new ManufacturerToMapSearchModel(), manufacturerSEOTemplate);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ManufacturerAddPopup(ManufacturerToMapSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return AccessDeniedView();

            var manufacturerSEOTemplate = await _manufacturerSEOTemplateService.GetManufacturerSEOTemplateByIdAsync(searchModel.ManufacturerSEOTemplateId);
            if (manufacturerSEOTemplate == null)
                return BadRequest();

            if (searchModel.SelectedManufacturerIds.Any())
            {
                foreach (var manufacturerId in searchModel.SelectedManufacturerIds)
                {
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);
                    if (manufacturer == null)
                        continue;

                    var manufacturer_ManufacturerSEOTemplate_Map = await _manufacturerManufacturerSEOTemplateMappingService.GetManufacturerManufacturerSEOTemplateMappingAsync(manufacturerSEOTemplate.Id, manufacturer.Id);

                    if (manufacturer_ManufacturerSEOTemplate_Map != null)
                        continue;

                    manufacturer_ManufacturerSEOTemplate_Map = new ManufacturerManufacturerSEOTemplateMapping()
                    {
                        ManufacturerId = manufacturer.Id,
                        ManufacturerSEOTemplateId = manufacturerSEOTemplate.Id,
                    };
                    await _manufacturerManufacturerSEOTemplateMappingService.InsertManufacturerManufacturerSEOTemplateMappingAsync(manufacturer_ManufacturerSEOTemplate_Map);
                }
            }

            ViewBag.RefreshPage = true;

            return View(new ManufacturerToMapSearchModel());
        }

        [HttpPost]
        public virtual async Task<IActionResult> GetManufacturerListToMap(ManufacturerToMapSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
                return await AccessDeniedDataTablesJson();

            var manufacturerSEOTemplate = await _manufacturerSEOTemplateService.GetManufacturerSEOTemplateByIdAsync(searchModel.ManufacturerSEOTemplateId);
            if (manufacturerSEOTemplate == null)
                return BadRequest();

            //prepare model
            var model = await _manufacturerManufacturerSEOTemplateMappingModelFactory.PrepareManufacturerToMapListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #endregion

        #endregion

        #endregion
    }
}
