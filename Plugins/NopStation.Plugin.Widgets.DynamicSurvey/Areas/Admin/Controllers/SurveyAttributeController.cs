using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;
using NopStation.Plugin.Widgets.DynamicSurvey.Services;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Controllers
{
    public partial class SurveyAttributeController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISurveyAttributeModelFactory _surveyAttributeModelFactory;
        private readonly ISurveyAttributeService _surveyAttributeService;

        #endregion Fields

        #region Ctor

        public SurveyAttributeController(ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISurveyAttributeModelFactory surveyAttributeModelFactory,
            ISurveyAttributeService surveyAttributeService)
        {
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _surveyAttributeModelFactory = surveyAttributeModelFactory;
            _surveyAttributeService = surveyAttributeService;
        }

        #endregion

        #region Utilities

        protected virtual async Task UpdateLocalesAsync(SurveyAttribute surveyAttribute, SurveyAttributeModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(surveyAttribute,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(surveyAttribute,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);
            }
        }

        protected virtual async Task UpdateLocalesAsync(PredefinedSurveyAttributeValue psav, PredefinedSurveyAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(psav,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        #region Attribute list / create / edit / delete

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //prepare model
            var model = await _surveyAttributeModelFactory.PrepareSurveyAttributeSearchModelAsync(new SurveyAttributeSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(SurveyAttributeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _surveyAttributeModelFactory.PrepareSurveyAttributeListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //prepare model
            var model = await _surveyAttributeModelFactory.PrepareSurveyAttributeModelAsync(new SurveyAttributeModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(SurveyAttributeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var surveyAttribute = model.ToEntity<SurveyAttribute>();
                await _surveyAttributeService.InsertSurveyAttributeAsync(surveyAttribute);
                await UpdateLocalesAsync(surveyAttribute, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.SurveyAttributes.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = surveyAttribute.Id });
            }

            //prepare model
            model = await _surveyAttributeModelFactory.PrepareSurveyAttributeModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute with the specified id
            var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(id);
            if (surveyAttribute == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _surveyAttributeModelFactory.PrepareSurveyAttributeModelAsync(null, surveyAttribute);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(SurveyAttributeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute with the specified id
            var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(model.Id);
            if (surveyAttribute == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                surveyAttribute = model.ToEntity(surveyAttribute);
                await _surveyAttributeService.UpdateSurveyAttributeAsync(surveyAttribute);

                await UpdateLocalesAsync(surveyAttribute, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.SurveyAttributes.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = surveyAttribute.Id });
            }

            //prepare model
            model = await _surveyAttributeModelFactory.PrepareSurveyAttributeModelAsync(model, surveyAttribute, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute with the specified id
            var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(id);
            if (surveyAttribute == null)
                return RedirectToAction("List");

            await _surveyAttributeService.DeleteSurveyAttributeAsync(surveyAttribute);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.SurveyAttributes.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var surveyAttributes = await _surveyAttributeService.GetSurveyAttributeByIdsAsync(selectedIds.ToArray());
            await _surveyAttributeService.DeleteSurveyAttributesAsync(surveyAttributes);

            return Json(new { Result = true });
        }

        #endregion

        #region Used by surveys

        [HttpPost]
        public virtual async Task<IActionResult> UsedBySurveys(SurveyAttributeSurveySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            //try to get a survey attribute with the specified id
            var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(searchModel.SurveyAttributeId)
                ?? throw new ArgumentException("No survey attribute found with the specified id");

            //prepare model
            var model = await _surveyAttributeModelFactory.PrepareSurveyAttributeSurveyListModelAsync(searchModel, surveyAttribute);

            return Json(model);
        }

        #endregion

        #region Predefined values

        [HttpPost]
        public virtual async Task<IActionResult> PredefinedSurveyAttributeValueList(PredefinedSurveyAttributeValueSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            //try to get a survey attribute with the specified id
            var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(searchModel.SurveyAttributeId)
                ?? throw new ArgumentException("No survey attribute found with the specified id");

            //prepare model
            var model = await _surveyAttributeModelFactory.PreparePredefinedSurveyAttributeValueListModelAsync(searchModel, surveyAttribute);

            return Json(model);
        }

        public virtual async Task<IActionResult> PredefinedSurveyAttributeValueCreatePopup(int surveyAttributeId)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute with the specified id
            var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(surveyAttributeId)
                ?? throw new ArgumentException("No survey attribute found with the specified id", nameof(surveyAttributeId));

            //prepare model
            var model = await _surveyAttributeModelFactory
                .PreparePredefinedSurveyAttributeValueModelAsync(new PredefinedSurveyAttributeValueModel(), surveyAttribute, null);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PredefinedSurveyAttributeValueCreatePopup(PredefinedSurveyAttributeValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute with the specified id
            var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(model.SurveyAttributeId)
                ?? throw new ArgumentException("No survey attribute found with the specified id");

            if (ModelState.IsValid)
            {
                //fill entity from model
                var psav = model.ToEntity<PredefinedSurveyAttributeValue>();

                await _surveyAttributeService.InsertPredefinedSurveyAttributeValueAsync(psav);
                await UpdateLocalesAsync(psav, model);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _surveyAttributeModelFactory.PreparePredefinedSurveyAttributeValueModelAsync(model, surveyAttribute, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> PredefinedSurveyAttributeValueEditPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a predefined survey attribute value with the specified id
            var surveyAttributeValue = await _surveyAttributeService.GetPredefinedSurveyAttributeValueByIdAsync(id)
                ?? throw new ArgumentException("No predefined survey attribute value found with the specified id");

            //try to get a survey attribute with the specified id
            var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(surveyAttributeValue.SurveyAttributeId)
                ?? throw new ArgumentException("No survey attribute found with the specified id");

            //prepare model
            var model = await _surveyAttributeModelFactory.PreparePredefinedSurveyAttributeValueModelAsync(null, surveyAttribute, surveyAttributeValue);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PredefinedSurveyAttributeValueEditPopup(PredefinedSurveyAttributeValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a predefined survey attribute value with the specified id
            var surveyAttributeValue = await _surveyAttributeService.GetPredefinedSurveyAttributeValueByIdAsync(model.Id)
                ?? throw new ArgumentException("No predefined survey attribute value found with the specified id");

            //try to get a survey attribute with the specified id
            var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(surveyAttributeValue.SurveyAttributeId)
                ?? throw new ArgumentException("No survey attribute found with the specified id");

            if (ModelState.IsValid)
            {
                surveyAttributeValue = model.ToEntity(surveyAttributeValue);
                await _surveyAttributeService.UpdatePredefinedSurveyAttributeValueAsync(surveyAttributeValue);

                await UpdateLocalesAsync(surveyAttributeValue, model);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _surveyAttributeModelFactory.PreparePredefinedSurveyAttributeValueModelAsync(model, surveyAttribute, surveyAttributeValue, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PredefinedSurveyAttributeValueDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a predefined survey attribute value with the specified id
            var surveyAttributeValue = await _surveyAttributeService.GetPredefinedSurveyAttributeValueByIdAsync(id)
                ?? throw new ArgumentException("No predefined survey attribute value found with the specified id", nameof(id));

            await _surveyAttributeService.DeletePredefinedSurveyAttributeValueAsync(surveyAttributeValue);

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}