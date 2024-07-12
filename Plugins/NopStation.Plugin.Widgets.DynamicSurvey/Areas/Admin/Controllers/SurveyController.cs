using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Controllers;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;
using NopStation.Plugin.Widgets.DynamicSurvey.Services;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Controllers
{
    public class SurveyController : BaseWidgetAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly INotificationService _notificationService;
        private readonly ISurveyModelFactory _surveyModelFactory;
        private readonly ISurveyService _surveyService;
        private readonly ICopySurveyService _copySurveyService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IAclService _aclService;
        private readonly ICustomerService _customerService;
        private readonly ISurveyAttributeService _surveyAttributeService;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly ISurveyAttributeParser _surveyAttributeParser;
        private readonly IConditionModelFactory _conditionModelFactory;

        #endregion

        #region Ctor

        public SurveyController(ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            INotificationService notificationService,
            ISurveyModelFactory surveyModelFactory,
            ISurveyService surveyService,
            ICopySurveyService copySurveyService,
            IPermissionService permissionService,
            IUrlRecordService urlRecordService,
            ILocalizedEntityService localizedEntityService,
            IAclService aclService,
            ICustomerService customerService,
            ISurveyAttributeService surveyAttributeService,
            IWorkContext workContext,
            ILanguageService languageService,
            ISurveyAttributeParser surveyAttributeParser,
            IConditionModelFactory conditionModelFactory)
        {
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _notificationService = notificationService;
            _surveyModelFactory = surveyModelFactory;
            _surveyService = surveyService;
            _copySurveyService = copySurveyService;
            _permissionService = permissionService;
            _urlRecordService = urlRecordService;
            _localizedEntityService = localizedEntityService;
            _aclService = aclService;
            _customerService = customerService;
            _surveyAttributeService = surveyAttributeService;
            _workContext = workContext;
            _languageService = languageService;
            _surveyAttributeParser = surveyAttributeParser;
            _conditionModelFactory = conditionModelFactory;
        }

        #endregion

        #region Utilities

        protected virtual async Task UpdateLocalesAsync(Survey survey, SurveyModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(survey,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(survey,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(survey,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(survey,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(survey,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(survey,
                    x => x.EmailAccountId,
                    localized.EmailAccountId,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(survey, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(survey, seName, localized.LanguageId);
            }
        }

        protected virtual async Task SaveSurveyAclAsync(Survey survey, SurveyModel model)
        {
            survey.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _surveyService.UpdateSurveyAsync(survey);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(survey);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                        await _aclService.InsertAclRecordAsync(survey, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        protected virtual async Task UpdateLocalesAsync(SurveyAttributeMapping sam, SurveyAttributeMappingModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(sam,
                    x => x.TextPrompt,
                    localized.TextPrompt,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(sam,
                    x => x.DefaultValue,
                    localized.DefaultValue,
                    localized.LanguageId);
            }
        }

        protected virtual async Task UpdateLocalesAsync(SurveyAttributeValue pav, SurveyAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(pav,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }

        protected virtual async Task SaveConditionAttributesAsync(SurveyAttributeMapping surveyAttributeMapping,
            SurveyAttributeConditionModel model, IFormCollection form)
        {
            string attributesXml = null;
            if (model.EnableCondition)
            {
                var attribute = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(model.SelectedSurveyAttributeId);
                if (attribute != null)
                {
                    var controlId = $"{DynamicSurveyDefaults.SurveyAttributePrefix}{attribute.Id}";
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                //for conditions we should empty values save even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _surveyAttributeParser.AddSurveyAttribute(null, attribute,
                                    selectedAttributeId > 0 ? selectedAttributeId.ToString() : string.Empty);
                            }
                            else
                            {
                                //for conditions we should empty values save even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _surveyAttributeParser.AddSurveyAttribute(null,
                                    attribute, string.Empty);
                            }

                            break;
                        case AttributeControlType.Checkboxes:
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                var anyValueSelected = false;
                                foreach (var item in cblAttributes.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId <= 0)
                                        continue;

                                    attributesXml = _surveyAttributeParser.AddSurveyAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                                    anyValueSelected = true;
                                }

                                if (!anyValueSelected)
                                {
                                    //for conditions we should save empty values even when nothing is selected
                                    //otherwise "attributesXml" will be empty
                                    //hence we won't be able to find a selected attribute
                                    attributesXml = _surveyAttributeParser.AddSurveyAttribute(null,
                                        attribute, string.Empty);
                                }
                            }
                            else
                            {
                                //for conditions we should save empty values even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _surveyAttributeParser.AddSurveyAttribute(null,
                                    attribute, string.Empty);
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are supported as conditions
                            break;
                    }
                }
            }

            surveyAttributeMapping.ConditionAttributeXml = attributesXml;
            await _surveyAttributeService.UpdateSurveyAttributeMappingAsync(surveyAttributeMapping);
        }

        #endregion

        #region Methods        

        #region Survey list / create / edit / delete

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var searchModel = await _surveyModelFactory.PrepareSurveySearchModelAsync(new SurveySearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(SurveySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            var model = await _surveyModelFactory.PrepareSurveyListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = await _surveyModelFactory.PrepareSurveyModelAsync(new SurveyModel(), null);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(SurveyModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var survey = model.ToEntity<Survey>();
                survey.CreatedOnUtc = DateTime.UtcNow;

                if (model.SendImmediately)
                    survey.DelayBeforeSend = null;

                await _surveyService.InsertSurveyAsync(survey);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(survey, model.SeName, survey.Name, true);
                await _urlRecordService.SaveSlugAsync(survey, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(survey, model);

                //ACL (customer roles)
                await SaveSurveyAclAsync(survey, model);

                //update schedule mappings
                await UpdateScheduleMappingsAsync(model.Schedule, survey, async (survey) => await _surveyService.UpdateSurveyAsync(survey));

                //stores
                await _surveyService.UpdateSurveyStoreMappingsAsync(survey, model.SelectedStoreIds);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = survey.Id });
            }

            model = await _surveyModelFactory.PrepareSurveyModelAsync(model, null);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var survey = await _surveyService.GetSurveyByIdAsync(id);
            if (survey == null || survey.Deleted)
                return RedirectToAction("List");

            var model = await _surveyModelFactory.PrepareSurveyModelAsync(null, survey);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(SurveyModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var survey = await _surveyService.GetSurveyByIdAsync(model.Id);
            if (survey == null || survey.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                survey = model.ToEntity(survey);

                if (model.SendImmediately)
                    survey.DelayBeforeSend = null;

                await _surveyService.UpdateSurveyAsync(survey);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(survey, model.SeName, survey.Name, true);
                await _urlRecordService.SaveSlugAsync(survey, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(survey, model);

                //ACL (customer roles)
                await SaveSurveyAclAsync(survey, model);

                //update schedule mappings
                await UpdateScheduleMappingsAsync(model.Schedule, survey, async (survey) => await _surveyService.UpdateSurveyAsync(survey));

                //stores
                await _surveyService.UpdateSurveyStoreMappingsAsync(survey, model.SelectedStoreIds);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = survey.Id });
            }

            model = await _surveyModelFactory.PrepareSurveyModelAsync(model, survey);
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var survey = await _surveyService.GetSurveyByIdAsync(id);
            if (survey == null || survey.Deleted)
                return RedirectToAction("List");

            await _surveyService.DeleteSurveyAsync(survey);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            await _surveyService.DeleteSurveysAsync((await _surveyService.GetSurveysByIdsAsync(selectedIds.ToArray())).ToList());

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> CopySurvey(SurveyModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var copyModel = model.CopySurveyModel;
            try
            {
                var originalSurvey = await _surveyService.GetSurveyByIdAsync(copyModel.Id);

                var newSurvey = await _copySurveyService.CopySurveyAsync(originalSurvey, copyModel.Name, copyModel.Published);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.Copied"));

                return RedirectToAction("Edit", new { id = newSurvey.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        #endregion

        #region Survey attributes

        [HttpPost]
        public virtual async Task<IActionResult> SurveyAttributeMappingList(SurveyAttributeMappingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(searchModel.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            //prepare model
            var model = await _surveyModelFactory.PrepareSurveyAttributeMappingListModelAsync(searchModel, survey);

            return Json(model);
        }

        public virtual async Task<IActionResult> SurveyAttributeMappingCreate(int surveyId)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            //prepare model
            var model = await _surveyModelFactory.PrepareSurveyAttributeMappingModelAsync(new SurveyAttributeMappingModel(), survey, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> SurveyAttributeMappingCreate(SurveyAttributeMappingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(model.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            //ensure this attribute is not mapped yet
            if ((await _surveyAttributeService.GetSurveyAttributeMappingsBySurveyIdAsync(survey.Id))
                .Any(x => x.SurveyAttributeId == model.SurveyAttributeId))
            {
                //redisplay form
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.AlreadyExists"));

                model = await _surveyModelFactory.PrepareSurveyAttributeMappingModelAsync(model, survey, null, true);

                return View(model);
            }

            //insert mapping
            var surveyAttributeMapping = model.ToEntity<SurveyAttributeMapping>();

            await _surveyAttributeService.InsertSurveyAttributeMappingAsync(surveyAttributeMapping);
            await UpdateLocalesAsync(surveyAttributeMapping, model);

            //predefined values
            var predefinedValues = await _surveyAttributeService.GetPredefinedSurveyAttributeValuesAsync(model.SurveyAttributeId);
            foreach (var predefinedValue in predefinedValues)
            {
                var pav = new SurveyAttributeValue
                {
                    SurveyAttributeMappingId = surveyAttributeMapping.Id,
                    Name = predefinedValue.Name,
                    IsPreSelected = predefinedValue.IsPreSelected,
                    DisplayOrder = predefinedValue.DisplayOrder
                };
                await _surveyAttributeService.InsertSurveyAttributeValueAsync(pav);

                //locales
                var languages = await _languageService.GetAllLanguagesAsync(true);

                //localization
                foreach (var lang in languages)
                {
                    var name = await _localizationService.GetLocalizedAsync(predefinedValue, x => x.Name, lang.Id, false, false);
                    if (!string.IsNullOrEmpty(name))
                        await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, name, lang.Id);
                }
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Added"));

            if (!continueEditing)
            {
                //select an appropriate card
                SaveSelectedCardName("survey-survey-attributes");
                return RedirectToAction("Edit", new { id = survey.Id });
            }

            return RedirectToAction("SurveyAttributeMappingEdit", new { id = surveyAttributeMapping.Id });
        }

        public virtual async Task<IActionResult> SurveyAttributeMappingEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute mapping with the specified id
            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(id)
                ?? throw new ArgumentException("No survey attribute mapping found with the specified id");

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyAttributeMapping.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            //prepare model
            var model = await _surveyModelFactory.PrepareSurveyAttributeMappingModelAsync(null, survey, surveyAttributeMapping);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> SurveyAttributeMappingEdit(SurveyAttributeMappingModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute mapping with the specified id
            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(model.Id)
                ?? throw new ArgumentException("No survey attribute mapping found with the specified id");

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyAttributeMapping.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            //ensure this attribute is not mapped yet
            if ((await _surveyAttributeService.GetSurveyAttributeMappingsBySurveyIdAsync(survey.Id))
                .Any(x => x.SurveyAttributeId == model.SurveyAttributeId && x.Id != surveyAttributeMapping.Id))
            {
                //redisplay form
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.AlreadyExists"));

                model = await _surveyModelFactory.PrepareSurveyAttributeMappingModelAsync(model, survey, surveyAttributeMapping, true);

                return View(model);
            }

            //fill entity from model
            surveyAttributeMapping = model.ToEntity(surveyAttributeMapping);
            await _surveyAttributeService.UpdateSurveyAttributeMappingAsync(surveyAttributeMapping);

            await UpdateLocalesAsync(surveyAttributeMapping, model);

            await SaveConditionAttributesAsync(surveyAttributeMapping, model.ConditionModel, form);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Updated"));

            if (!continueEditing)
            {
                //select an appropriate card
                SaveSelectedCardName("survey-survey-attributes");
                return RedirectToAction("Edit", new { id = survey.Id });
            }

            return RedirectToAction("SurveyAttributeMappingEdit", new { id = surveyAttributeMapping.Id });
        }

        [HttpPost]
        public virtual async Task<IActionResult> SurveyAttributeMappingDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute mapping with the specified id
            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(id)
                ?? throw new ArgumentException("No survey attribute mapping found with the specified id");

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyAttributeMapping.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            await _surveyAttributeService.DeleteSurveyAttributeMappingAsync(surveyAttributeMapping);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Deleted"));

            //select an appropriate card
            SaveSelectedCardName("survey-survey-attributes");
            return RedirectToAction("Edit", new { id = surveyAttributeMapping.SurveyId });
        }

        [HttpPost]
        public virtual async Task<IActionResult> SurveyAttributeValueList(SurveyAttributeValueSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            //try to get a survey attribute mapping with the specified id
            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(searchModel.SurveyAttributeMappingId)
                ?? throw new ArgumentException("No survey attribute mapping found with the specified id");

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyAttributeMapping.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            //prepare model
            var model = await _surveyModelFactory.PrepareSurveyAttributeValueListModelAsync(searchModel, surveyAttributeMapping);

            return Json(model);
        }

        public virtual async Task<IActionResult> SurveyAttributeValueCreatePopup(int surveyAttributeMappingId)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute mapping with the specified id
            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(surveyAttributeMappingId)
                ?? throw new ArgumentException("No survey attribute mapping found with the specified id");

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyAttributeMapping.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            //prepare model
            var model = await _surveyModelFactory.PrepareSurveyAttributeValueModelAsync(new SurveyAttributeValueModel(), surveyAttributeMapping, null);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SurveyAttributeValueCreatePopup(SurveyAttributeValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute mapping with the specified id
            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(model.SurveyAttributeMappingId);
            if (surveyAttributeMapping == null)
                return RedirectToAction("List", "Survey");

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyAttributeMapping.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            if (surveyAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError(string.Empty, "Color is required");
                try
                {
                    //ensure color is valid (can be instantiated)
                    System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.Message);
                }
            }

            //ensure a picture is uploaded
            if (surveyAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
            {
                ModelState.AddModelError(string.Empty, "Image is required");
            }

            if (ModelState.IsValid)
            {
                //fill entity from model
                var pav = model.ToEntity<SurveyAttributeValue>();

                await _surveyAttributeService.InsertSurveyAttributeValueAsync(pav);
                await UpdateLocalesAsync(pav, model);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _surveyModelFactory.PrepareSurveyAttributeValueModelAsync(model, surveyAttributeMapping, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> SurveyAttributeValueEditPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute value with the specified id
            var surveyAttributeValue = await _surveyAttributeService.GetSurveyAttributeValueByIdAsync(id);
            if (surveyAttributeValue == null)
                return RedirectToAction("List", "Survey");

            //try to get a survey attribute mapping with the specified id
            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(surveyAttributeValue.SurveyAttributeMappingId);
            if (surveyAttributeMapping == null)
                return RedirectToAction("List", "Survey");

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyAttributeMapping.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            //prepare model
            var model = await _surveyModelFactory.PrepareSurveyAttributeValueModelAsync(null, surveyAttributeMapping, surveyAttributeValue);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SurveyAttributeValueEditPopup(SurveyAttributeValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute value with the specified id
            var surveyAttributeValue = await _surveyAttributeService.GetSurveyAttributeValueByIdAsync(model.Id);
            if (surveyAttributeValue == null)
                return RedirectToAction("List", "Survey");

            //try to get a survey attribute mapping with the specified id
            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(surveyAttributeValue.SurveyAttributeMappingId);
            if (surveyAttributeMapping == null)
                return RedirectToAction("List", "Survey");

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyAttributeMapping.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            if (surveyAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError(string.Empty, "Color is required");
                try
                {
                    //ensure color is valid (can be instantiated)
                    System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.Message);
                }
            }

            //ensure a picture is uploaded
            if (surveyAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
            {
                ModelState.AddModelError(string.Empty, "Image is required");
            }

            if (ModelState.IsValid)
            {
                //fill entity from model
                surveyAttributeValue = model.ToEntity(surveyAttributeValue);
                await _surveyAttributeService.UpdateSurveyAttributeValueAsync(surveyAttributeValue);

                await UpdateLocalesAsync(surveyAttributeValue, model);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _surveyModelFactory.PrepareSurveyAttributeValueModelAsync(model, surveyAttributeMapping, surveyAttributeValue, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SurveyAttributeValueDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey attribute value with the specified id
            var surveyAttributeValue = await _surveyAttributeService.GetSurveyAttributeValueByIdAsync(id)
                ?? throw new ArgumentException("No survey attribute value found with the specified id");

            //try to get a survey attribute mapping with the specified id
            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(surveyAttributeValue.SurveyAttributeMappingId)
                ?? throw new ArgumentException("No survey attribute mapping found with the specified id");

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(surveyAttributeMapping.SurveyId)
                ?? throw new ArgumentException("No survey found with the specified id");

            await _surveyAttributeService.DeleteSurveyAttributeValueAsync(surveyAttributeValue);

            return new NullJsonResult();
        }

        #endregion

        #region Widget zone mappings

        [HttpPost]
        public virtual async Task<IActionResult> WidgetZoneList(WidgetZoneSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            var survey = await _surveyService.GetSurveyByIdAsync(searchModel.EntityId);
            if (survey == null || survey.Deleted)
                throw new ArgumentException("No survey found with the specified id");

            var model = await base.WidgetZoneListAsync(searchModel, survey);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> WidgetZoneCreate(WidgetZoneModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            var survey = await _surveyService.GetSurveyByIdAsync(model.EntityId);
            if (survey == null || survey.Deleted)
                throw new ArgumentException("No survey found with the specified id");

            await base.WidgetZoneCreateAsync(model, survey);

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> WidgetZoneEdit(WidgetZoneModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            var survey = await _surveyService.GetSurveyByIdAsync(model.EntityId);
            if (survey == null || survey.Deleted)
                throw new ArgumentException("No survey found with the specified id");

            await base.WidgetZoneEditAsync(model, survey);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> WidgetZoneDelete(int id, int entityId)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            var survey = await _surveyService.GetSurveyByIdAsync(entityId);
            if (survey == null || survey.Deleted)
                throw new ArgumentException("No survey found with the specified id");

            await base.WidgetZoneDeleteAsync(id, survey);

            return new NullJsonResult();
        }

        #endregion

        #region Survey submissions

        [HttpPost]
        public virtual async Task<IActionResult> GetSurveySubmissionList(SurveySubmissionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            var model = await _surveyModelFactory.PrepareSurveySubmissionListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Condition mappings

        [HttpPost]
        public virtual async Task<IActionResult> CustomerConditionList(CustomerConditionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(searchModel.EntityId);
            if (survey == null || survey.Deleted)
                throw new ArgumentException("No survey found with the specified id");

            //prepare model
            var model = await base.CustomerConditionListAsync(searchModel, survey);

            return Json(model);
        }

        public virtual async Task<IActionResult> CustomerConditionDelete(int id, int entityId)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(entityId);
            if (survey == null || survey.Deleted)
                throw new ArgumentException("No survey found with the specified id");

            await base.CustomerConditionDeleteAsync(id, survey);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> CustomerConditionAddPopup(int entityId)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(entityId);
            if (survey == null || survey.Deleted)
                throw new ArgumentException("No survey found with the specified id");

            //prepare model
            var model = await _conditionModelFactory.PrepareAddCustomerToConditionSearchModelAsync(new AddCustomerToConditionSearchModel(), survey);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CustomerConditionAddPopupList(AddCustomerToConditionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _conditionModelFactory.PrepareAddCustomerToConditionListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> CustomerConditionAddPopup(AddCustomerToConditionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            //try to get a survey with the specified id
            var survey = await _surveyService.GetSurveyByIdAsync(model.EntityId);
            if (survey == null || survey.Deleted)
                throw new ArgumentException("No survey found with the specified id");

            await base.CustomerConditionAddAsync(model, survey);

            ViewBag.RefreshPage = true;

            return View(new AddCustomerToConditionSearchModel());
        }

        #endregion

        #endregion
    }
}
