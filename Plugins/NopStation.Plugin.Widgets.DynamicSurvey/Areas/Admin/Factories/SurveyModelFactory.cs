using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;
using NopStation.Plugin.Widgets.DynamicSurvey.Services;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Factories
{
    public partial class SurveyModelFactory : ISurveyModelFactory
    {
        #region Fields

        private readonly Nop.Core.Domain.Catalog.CatalogSettings _catalogSettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ISurveyAttributeParser _surveyAttributeParser;
        private readonly ISurveyAttributeService _surveyAttributeService;
        private readonly ISurveyService _surveyService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWidgetZoneModelFactory _widgetZoneMappingModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerService _customerService;
        private readonly IScheduleModelFactory _scheduleMappingModelFactory;
        private readonly IConditionModelFactory _conditionMappingModelFactory;

        #endregion

        #region Ctor

        public SurveyModelFactory(Nop.Core.Domain.Catalog.CatalogSettings catalogSettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            ISurveyAttributeParser surveyAttributeParser,
            ISurveyAttributeService surveyAttributeService,
            ISurveyService surveyService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IUrlRecordService urlRecordService,
            IWidgetZoneModelFactory widgetZoneMappingModelFactory,
            IDateTimeHelper dateTimeHelper,
            ICustomerService customerService,
            IScheduleModelFactory scheduleMappingModelFactory,
            IConditionModelFactory conditionMappingModelFactory)
        {
            _catalogSettings = catalogSettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _surveyAttributeParser = surveyAttributeParser;
            _surveyAttributeService = surveyAttributeService;
            _surveyService = surveyService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _urlRecordService = urlRecordService;
            _widgetZoneMappingModelFactory = widgetZoneMappingModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _customerService = customerService;
            _scheduleMappingModelFactory = scheduleMappingModelFactory;
            _conditionMappingModelFactory = conditionMappingModelFactory;
        }

        #endregion

        #region Utilities

        protected virtual async Task<CopySurveyModel> PrepareCopySurveyModelAsync(CopySurveyModel model, Survey survey)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Id = survey.Id;
            model.Name = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.Copy.Name.New"), survey.Name);
            model.Published = true;

            return model;
        }

        protected virtual async Task<string> PrepareSurveyAttributeMappingValidationRulesStringAsync(SurveyAttributeMapping attributeMapping)
        {
            if (!attributeMapping.ValidationRulesAllowed())
                return string.Empty;

            var validationRules = new StringBuilder(string.Empty);

            if (attributeMapping.AttributeControlType == AttributeControlType.Datepicker)
            {
                if (attributeMapping.DefaultDate.HasValue)
                    validationRules.AppendFormat("{0}: {1}<br />",
                        await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.DefaultDate"),
                        attributeMapping.DefaultDate.Value.ToString("D"));

                if (attributeMapping.ValidationMinDate.HasValue)
                    validationRules.AppendFormat("{0}: {1}<br />",
                        await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MinDate"),
                        attributeMapping.ValidationMinDate.Value.ToString("D"));

                if (attributeMapping.ValidationMaxDate.HasValue)
                    validationRules.AppendFormat("{0}: {1}<br />",
                        await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MaxDate"),
                        attributeMapping.ValidationMaxDate.Value.ToString("D"));
            }
            else
            {
                if (attributeMapping.ValidationMinLength.HasValue)
                    validationRules.AppendFormat("{0}: {1}<br />",
                        await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MinLength"),
                        attributeMapping.ValidationMinLength);

                if (attributeMapping.ValidationMaxLength.HasValue)
                    validationRules.AppendFormat("{0}: {1}<br />",
                        await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MaxLength"),
                        attributeMapping.ValidationMaxLength);

                if (!string.IsNullOrEmpty(attributeMapping.ValidationFileAllowedExtensions))
                    validationRules.AppendFormat("{0}: {1}<br />",
                        await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.FileAllowedExtensions"),
                        WebUtility.HtmlEncode(attributeMapping.ValidationFileAllowedExtensions));

                if (attributeMapping.ValidationFileMaximumSize.HasValue)
                    validationRules.AppendFormat("{0}: {1}<br />",
                        await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.FileMaximumSize"),
                        attributeMapping.ValidationFileMaximumSize);

                if (!string.IsNullOrEmpty(attributeMapping.DefaultValue))
                    validationRules.AppendFormat("{0}: {1}<br />",
                        await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.DefaultValue"),
                        WebUtility.HtmlEncode(attributeMapping.DefaultValue));
            }

            return validationRules.ToString();
        }

        protected virtual async Task PrepareSurveyAttributeConditionModelAsync(SurveyAttributeConditionModel model,
            SurveyAttributeMapping surveyAttributeMapping)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (surveyAttributeMapping == null)
                throw new ArgumentNullException(nameof(surveyAttributeMapping));

            model.SurveyAttributeMappingId = surveyAttributeMapping.Id;
            model.EnableCondition = !string.IsNullOrEmpty(surveyAttributeMapping.ConditionAttributeXml);

            //pre-select attribute and values
            var selectedPva = (await _surveyAttributeParser
                .ParseSurveyAttributeMappingsAsync(surveyAttributeMapping.ConditionAttributeXml))
                .FirstOrDefault();

            var attributes = (await _surveyAttributeService.GetSurveyAttributeMappingsBySurveyIdAsync(surveyAttributeMapping.SurveyId))
                //ignore non-combinable attributes (should have selectable values)
                .Where(x => x.CanBeUsedAsCondition())
                //ignore this attribute (it cannot depend on itself)
                .Where(x => x.Id != surveyAttributeMapping.Id)
                .ToList();
            foreach (var attribute in attributes)
            {
                var attributeModel = new SurveyAttributeConditionModel.SurveyAttributeModel
                {
                    Id = attribute.Id,
                    SurveyAttributeId = attribute.SurveyAttributeId,
                    Name = (await _surveyAttributeService.GetSurveyAttributeByIdAsync(attribute.SurveyAttributeId)).Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _surveyAttributeService.GetSurveyAttributeValuesAsync(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new SurveyAttributeConditionModel.SurveyAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }

                    //pre-select attribute and value
                    if (selectedPva != null && attribute.Id == selectedPva.Id)
                    {
                        //attribute
                        model.SelectedSurveyAttributeId = selectedPva.Id;

                        //values
                        switch (attribute.AttributeControlType)
                        {
                            case AttributeControlType.DropdownList:
                            case AttributeControlType.RadioList:
                            case AttributeControlType.Checkboxes:
                            case AttributeControlType.ColorSquares:
                            case AttributeControlType.ImageSquares:
                                if (!string.IsNullOrEmpty(surveyAttributeMapping.ConditionAttributeXml))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues =
                                        await _surveyAttributeParser.ParseSurveyAttributeValuesAsync(surveyAttributeMapping
                                            .ConditionAttributeXml);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;
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

                model.SurveyAttributes.Add(attributeModel);
            }
        }

        protected virtual SurveyAttributeMappingSearchModel PrepareSurveyAttributeMappingSearchModel(SurveyAttributeMappingSearchModel searchModel,
            Survey survey)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            searchModel.SurveyId = survey.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual SurveyAttributeValueSearchModel PrepareSurveyAttributeValueSearchModel(SurveyAttributeValueSearchModel searchModel,
            SurveyAttributeMapping surveyAttributeMapping)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (surveyAttributeMapping == null)
                throw new ArgumentNullException(nameof(surveyAttributeMapping));

            searchModel.SurveyAttributeMappingId = surveyAttributeMapping.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        #region Surveys

        public virtual async Task<SurveySearchModel> PrepareSurveySearchModelAsync(SurveySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.List.SearchPublished.All")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.List.SearchPublished.PublishedOnly")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.List.SearchPublished.UnpublishedOnly")
            });

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<SurveyListModel> PrepareSurveyListModelAsync(SurveySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);

            //get surveys
            var surveys = (await _surveyService.SearchSurveysAsync(showHidden: true,
                overridePublished: overridePublished,
                storeId: searchModel.SearchStoreId,
                keywords: searchModel.SearchSurveyName)).ToPagedList(searchModel);

            //prepare list model
            var model = await new SurveyListModel().PrepareToGridAsync(searchModel, surveys, () =>
            {
                return surveys.SelectAwait(async survey =>
                {
                    //fill in model values from the entity
                    var surveyModel = survey.ToModel<SurveyModel>();

                    //little performance optimization: ensure that "Description" is not returned
                    surveyModel.Description = string.Empty;

                    //fill in additional values (not existing in the entity)
                    surveyModel.SeName = await _urlRecordService.GetSeNameAsync(survey, 0, true, false);

                    return surveyModel;
                });
            });

            return model;
        }

        public virtual async Task<SurveyModel> PrepareSurveyModelAsync(SurveyModel model, Survey survey, bool excludeProperties = false)
        {
            Func<SurveyLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (survey != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = survey.ToModel<SurveyModel>();
                    model.SeName = await _urlRecordService.GetSeNameAsync(survey, 0, true, false);
                }

                model.SurveyAttributesExist = (await _surveyAttributeService.GetAllSurveyAttributesAsync()).Any();

                //prepare copy survey model
                await PrepareCopySurveyModelAsync(model.CopySurveyModel, survey);

                //prepare nested search model
                PrepareSurveyAttributeMappingSearchModel(model.SurveyAttributeMappingSearchModel, survey);

                //prepare submission search model
                await PrepareSurveySubmissionSearchModelAsync(model.SurveySubmissionSearchModel, survey);

                //prepare customer condition mapping model
                await _conditionMappingModelFactory.PrepareCustomerConditionMappingSearchModelAsync(model, survey);

                //prepare wiget zone mapping model
                await _widgetZoneMappingModelFactory.PrepareWidgetZoneMappingSearchModelAsync(model, survey);
                await _widgetZoneMappingModelFactory.PrepareAddWidgetZoneMappingModelAsync(model, survey);
                await _widgetZoneMappingModelFactory.PrepareWidgetZonesAsync(model.AddWidgetZoneModel.AvaliableWidgetZones, SurveyHelper.GetWidgetZones(), false);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(survey, entity => entity.Name, languageId, false, false);
                    locale.Description = await _localizationService.GetLocalizedAsync(survey, entity => entity.Description, languageId, false, false);
                    locale.MetaKeywords = await _localizationService.GetLocalizedAsync(survey, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = await _localizationService.GetLocalizedAsync(survey, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = await _localizationService.GetLocalizedAsync(survey, entity => entity.MetaTitle, languageId, false, false);
                    locale.MetaTitle = await _localizationService.GetLocalizedAsync(survey, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = await _urlRecordService.GetSeNameAsync(survey, languageId, false, false);
                    locale.EmailAccountId = await _localizationService.GetLocalizedAsync(survey, entity => entity.EmailAccountId, languageId, false, false);

                    //prepare available email accounts
                    await _baseAdminModelFactory.PrepareEmailAccountsAsync(locale.AvailableEmailAccounts,
                        defaultItemText: await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.Fields.EmailAccount.Standard"));

                    //PrepareEmailAccounts only gets available accounts, we need to set the item as selected manually
                    if (locale.AvailableEmailAccounts?.FirstOrDefault(x => x.Value == locale.EmailAccountId.ToString()) is SelectListItem emailAccountListItem)
                    {
                        emailAccountListItem.Selected = true;
                    }
                };
            }
            model.SendImmediately = !model.DelayBeforeSend.HasValue;

            //set default values for the new model
            if (survey == null)
            {
                model.Published = true;
                model.EnableEmailing = true;
                model.AllowMultipleSubmissions = true;
            }

            //prepare model schedule mappings
            await _scheduleMappingModelFactory.PrepareScheduleMappingModelAsync(model, survey);

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, survey, excludeProperties);

            //prepare available email accounts
            await _baseAdminModelFactory.PrepareEmailAccountsAsync(model.AvailableEmailAccounts);

            //prepare model stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, survey, excludeProperties);

            return model;
        }

        #endregion

        #region Survey attribute mappings

        public virtual async Task<SurveyAttributeMappingListModel> PrepareSurveyAttributeMappingListModelAsync(SurveyAttributeMappingSearchModel searchModel,
            Survey survey)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            //get survey attribute mappings
            var surveyAttributeMappings = (await _surveyAttributeService
                .GetSurveyAttributeMappingsBySurveyIdAsync(survey.Id)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new SurveyAttributeMappingListModel().PrepareToGridAsync(searchModel, surveyAttributeMappings, () =>
            {
                return surveyAttributeMappings.SelectAwait(async attributeMapping =>
                {
                    //fill in model values from the entity
                    var surveyAttributeMappingModel = attributeMapping.ToModel<SurveyAttributeMappingModel>();

                    //fill in additional values (not existing in the entity)
                    surveyAttributeMappingModel.ConditionString = string.Empty;

                    surveyAttributeMappingModel.ValidationRulesString = await PrepareSurveyAttributeMappingValidationRulesStringAsync(attributeMapping);
                    surveyAttributeMappingModel.SurveyAttribute = (await _surveyAttributeService.GetSurveyAttributeByIdAsync(attributeMapping.SurveyAttributeId))?.Name;
                    surveyAttributeMappingModel.AttributeControlType = await _localizationService.GetLocalizedEnumAsync(attributeMapping.AttributeControlType);
                    var conditionAttribute = (await _surveyAttributeParser
                        .ParseSurveyAttributeMappingsAsync(attributeMapping.ConditionAttributeXml))
                        .FirstOrDefault();
                    if (conditionAttribute == null)
                        return surveyAttributeMappingModel;

                    var conditionValue = (await _surveyAttributeParser
                        .ParseSurveyAttributeValuesAsync(attributeMapping.ConditionAttributeXml))
                        .FirstOrDefault();
                    if (conditionValue != null)
                        surveyAttributeMappingModel.ConditionString =
                            $"{WebUtility.HtmlEncode((await _surveyAttributeService.GetSurveyAttributeByIdAsync(conditionAttribute.SurveyAttributeId)).Name)}: {WebUtility.HtmlEncode(conditionValue.Name)}";

                    return surveyAttributeMappingModel;
                });
            });

            return model;
        }

        public virtual async Task<SurveyAttributeMappingModel> PrepareSurveyAttributeMappingModelAsync(SurveyAttributeMappingModel model,
            Survey survey, SurveyAttributeMapping surveyAttributeMapping, bool excludeProperties = false)
        {
            Func<SurveyAttributeMappingLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            if (surveyAttributeMapping != null)
            {
                //fill in model values from the entity
                model ??= new SurveyAttributeMappingModel
                {
                    Id = surveyAttributeMapping.Id
                };

                model.SurveyAttribute = (await _surveyAttributeService.GetSurveyAttributeByIdAsync(surveyAttributeMapping.SurveyAttributeId)).Name;
                model.AttributeControlType = await _localizationService.GetLocalizedEnumAsync(surveyAttributeMapping.AttributeControlType);

                if (!excludeProperties)
                {
                    model.SurveyAttributeId = surveyAttributeMapping.SurveyAttributeId;
                    model.TextPrompt = surveyAttributeMapping.TextPrompt;
                    model.IsRequired = surveyAttributeMapping.IsRequired;
                    model.AttributeControlTypeId = surveyAttributeMapping.AttributeControlTypeId;
                    model.DisplayOrder = surveyAttributeMapping.DisplayOrder;
                    model.ValidationMinLength = surveyAttributeMapping.ValidationMinLength;
                    model.ValidationMaxLength = surveyAttributeMapping.ValidationMaxLength;
                    model.ValidationMinDate = surveyAttributeMapping.ValidationMinDate;
                    model.DefaultDate = surveyAttributeMapping.DefaultDate;
                    model.ValidationMaxDate = surveyAttributeMapping.ValidationMaxDate;
                    model.ValidationFileAllowedExtensions = surveyAttributeMapping.ValidationFileAllowedExtensions;
                    model.ValidationFileMaximumSize = surveyAttributeMapping.ValidationFileMaximumSize;
                    model.DefaultValue = surveyAttributeMapping.DefaultValue;
                }

                //prepare condition attributes model
                model.ConditionAllowed = true;
                await PrepareSurveyAttributeConditionModelAsync(model.ConditionModel, surveyAttributeMapping);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.TextPrompt = await _localizationService.GetLocalizedAsync(surveyAttributeMapping, entity => entity.TextPrompt, languageId, false, false);
                    locale.DefaultValue = await _localizationService.GetLocalizedAsync(surveyAttributeMapping, entity => entity.DefaultValue, languageId, false, false);
                };

                //prepare nested search model
                PrepareSurveyAttributeValueSearchModel(model.SurveyAttributeValueSearchModel, surveyAttributeMapping);
            }

            model.SurveyId = survey.Id;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare available survey attributes
            model.AvailableSurveyAttributes = (await _surveyAttributeService.GetAllSurveyAttributesAsync()).Select(surveyAttribute => new SelectListItem
            {
                Text = surveyAttribute.Name,
                Value = surveyAttribute.Id.ToString()
            }).ToList();

            return model;
        }

        public virtual async Task<SurveyAttributeValueListModel> PrepareSurveyAttributeValueListModelAsync(SurveyAttributeValueSearchModel searchModel,
            SurveyAttributeMapping surveyAttributeMapping)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (surveyAttributeMapping == null)
                throw new ArgumentNullException(nameof(surveyAttributeMapping));

            //get survey attribute values
            var surveyAttributeValues = (await _surveyAttributeService
                .GetSurveyAttributeValuesAsync(surveyAttributeMapping.Id)).ToPagedList(searchModel);

            //prepare list model
            var model = new SurveyAttributeValueListModel().PrepareToGrid(searchModel, surveyAttributeValues, () =>
            {
                return surveyAttributeValues.Select(value =>
                {
                    //fill in model values from the entity
                    var productAttributeValueModel = value.ToModel<SurveyAttributeValueModel>();
                    productAttributeValueModel.Name = surveyAttributeMapping.AttributeControlType != AttributeControlType.ColorSquares
                        ? value.Name : $"{value.Name} - {value.ColorSquaresRgb}";

                    return productAttributeValueModel;
                });
            });

            return model;
        }

        public virtual async Task<SurveyAttributeValueModel> PrepareSurveyAttributeValueModelAsync(SurveyAttributeValueModel model,
            SurveyAttributeMapping surveyAttributeMapping, SurveyAttributeValue surveyAttributeValue, bool excludeProperties = false)
        {
            if (surveyAttributeMapping == null)
                throw new ArgumentNullException(nameof(surveyAttributeMapping));

            Func<SurveyAttributeValueLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (surveyAttributeValue != null)
            {
                //fill in model values from the entity
                model ??= new SurveyAttributeValueModel
                {
                    SurveyAttributeMappingId = surveyAttributeValue.SurveyAttributeMappingId,
                    Name = surveyAttributeValue.Name,
                    ColorSquaresRgb = surveyAttributeValue.ColorSquaresRgb,
                    DisplayColorSquaresRgb = surveyAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares,
                    ImageSquaresPictureId = surveyAttributeValue.ImageSquaresPictureId,
                    DisplayImageSquaresPicture = surveyAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares,
                    IsPreSelected = surveyAttributeValue.IsPreSelected,
                    DisplayOrder = surveyAttributeValue.DisplayOrder
                };

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(surveyAttributeValue, entity => entity.Name, languageId, false, false);
                };
            }

            model.SurveyAttributeMappingId = surveyAttributeMapping.Id;
            model.DisplayColorSquaresRgb = surveyAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares;
            model.DisplayImageSquaresPicture = surveyAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }

        #endregion

        #region Survey submissions

        public virtual async Task<SurveySubmissionSearchModel> PrepareSurveySubmissionSearchModelAsync(SurveySubmissionSearchModel searchModel, Survey survey, bool excludeProperties = true)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare grid
            searchModel.SetGridPageSize();
            searchModel.SurveyId = survey?.Id ?? 0;

            if (!excludeProperties)
            {
                searchModel.AvailableSurveys = (await _surveyService.SearchSurveysAsync())
                    .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                    .ToList();

                searchModel.AvailableSurveys.Insert(0, new SelectListItem(await _localizationService.GetResourceAsync("Admin.Common.All"), "0"));
            }

            return searchModel;
        }

        public virtual async Task<SurveySubmissionListModel> PrepareSurveySubmissionListModelAsync(SurveySubmissionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get surveys
            var surveySubmissions = await _surveyService.GetAllSurveySubmissionsAsync(
                surveyId: searchModel.SurveyId,
                customerEmail: searchModel.SearchCustomerEmail,
                dateStart: searchModel.DateStart,
                dateEnd: searchModel.DateEnd,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new SurveySubmissionListModel().PrepareToGridAsync(searchModel, surveySubmissions, () =>
            {
                return surveySubmissions.SelectAwait(async surveySubmission =>
                {
                    return await PrepareSurveySubmissionModelAsync(null, surveySubmission);
                });
            });

            return model;
        }

        public virtual async Task<SurveySubmissionModel> PrepareSurveySubmissionModelAsync(SurveySubmissionModel model, SurveySubmission surveySubmission, bool excludeProperties = true)
        {
            if (surveySubmission != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = surveySubmission.ToModel<SurveySubmissionModel>();

                    var customer = await _customerService.GetCustomerByIdAsync(surveySubmission.CustomerId);
                    if (customer != null && await _customerService.IsRegisteredAsync(customer))
                        model.CustomerEmail = customer.Email;
                    else
                        model.CustomerEmail = await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.SurveySubmissions.Guest");

                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(surveySubmission.CreatedOnUtc);
                }

                if (!excludeProperties)
                {
                    var survey = await _surveyService.GetSurveyByIdAsync(surveySubmission.SurveyId);

                    if (survey != null)
                        model.SurveyName = await _localizationService.GetLocalizedAsync(survey, x => x.Name);
                }
            }

            return model;
        }

        #endregion

        #endregion
    }
}