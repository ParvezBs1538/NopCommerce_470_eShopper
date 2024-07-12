using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Controllers;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;
using NopStation.Plugin.Widgets.DynamicSurvey.Services;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Controllers
{
    public class SurveySubmissionController : BaseWidgetAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISurveyService _surveyService;
        private readonly ISurveyModelFactory _surveyModelFactory;

        #endregion

        #region Ctor

        public SurveySubmissionController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISurveyService surveyService,
            ISurveyModelFactory surveyModelFactory)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _surveyService = surveyService;
            _surveyModelFactory = surveyModelFactory;
        }

        #endregion

        #region Methods

        #region List/details/delete

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var model = await _surveyModelFactory.PrepareSurveySubmissionSearchModelAsync(new SurveySubmissionSearchModel(), null, false);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(SurveySubmissionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            var model = await _surveyModelFactory.PrepareSurveySubmissionListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var surveySubmission = await _surveyService.GetSurveySubmissionByIdAsync(id);

            if (surveySubmission == null)
                return RedirectToAction("List");

            var model = await _surveyModelFactory.PrepareSurveySubmissionModelAsync(null, surveySubmission, false);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSelected(int[] selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            if (selectedIds == null || !selectedIds.Any())
                return RedirectToAction("List");

            var surveySubmissions = await _surveyService.GetSurveySubmissionsByIdsAsync(selectedIds);
            await _surveyService.DeleteSurveySubmissionsAsync(surveySubmissions);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.SurveySubmissions.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var surveySubmission = await _surveyService.GetSurveySubmissionByIdAsync(id);

            if (surveySubmission == null)
                return RedirectToAction("List");

            await _surveyService.DeleteSurveySubmissionAsync(surveySubmission);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.SurveySubmissions.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Export

        [HttpPost, ActionName("ExportExcel")]
        [FormValueRequired("exportexcel-all")]
        public virtual async Task<IActionResult> ExportExcelAll(SurveySubmissionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //get surveys
            var surveySubmissions = await _surveyService.GetAllSurveySubmissionsAsync(
                surveyId: searchModel.SurveyId,
                customerId: searchModel.CustomerId,
                dateStart: searchModel.DateStart,
                dateEnd: searchModel.DateEnd,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //ensure that we at least one order selected
            if (!surveySubmissions.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.ExportNoSurveySubmission"));
                return RedirectToAction("List");
            }

            try
            {
                var bytes = await _surveyService.ExportSurveysToXlsxAsync(surveySubmissions);
                return File(bytes, MimeTypes.TextXlsx, "survey-submissions.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ExportExcelSelected(int[] selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var surveys = new List<SurveySubmission>();
            if (selectedIds != null)
            {
                surveys.AddRange(await _surveyService.GetSurveySubmissionsByIdsAsync(selectedIds));
            }

            try
            {
                var bytes = await _surveyService.ExportSurveysToXlsxAsync(surveys);
                return File(bytes, MimeTypes.TextXlsx, "survey-submissions.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }


        #endregion

        #endregion
    }
}
