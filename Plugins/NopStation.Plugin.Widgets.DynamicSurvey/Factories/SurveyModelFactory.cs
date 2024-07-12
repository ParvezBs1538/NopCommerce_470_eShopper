using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;
using NopStation.Plugin.Widgets.DynamicSurvey.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Services;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Factories
{
    public class SurveyModelFactory : ISurveyModelFactory
    {
        #region Fields

        private readonly ISurveyService _surveyService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreContext _storeContext;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISurveyAttributeService _surveyAttributeService;
        private readonly ISurveyAttributeParser _surveyAttributeParser;
        private readonly IWebHelper _webHelper;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IPictureService _pictureService;
        private readonly DynamicSurveySettings _dynamicSurveySettings;

        #endregion

        #region Ctor

        public SurveyModelFactory(ISurveyService surveyService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IStoreContext storeContext,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            ISurveyAttributeService surveyAttributeService,
            ISurveyAttributeParser surveyAttributeParser,
            IWebHelper webHelper,
            IStaticCacheManager staticCacheManager,
            IPictureService pictureService,
            DynamicSurveySettings dynamicSurveySettings)
        {
            _surveyService = surveyService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _storeContext = storeContext;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _surveyAttributeService = surveyAttributeService;
            _surveyAttributeParser = surveyAttributeParser;
            _webHelper = webHelper;
            _staticCacheManager = staticCacheManager;
            _pictureService = pictureService;
            _dynamicSurveySettings = dynamicSurveySettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task<SurveyModel> PrepareSurveyModelAsync(Survey survey, string attributesXml = "")
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            var model = new SurveyModel
            {
                Id = survey.Id,
                SystemName = survey.SystemName,
                Name = await _localizationService.GetLocalizedAsync(survey, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(survey, x => x.Description),
                MetaKeywords = await _localizationService.GetLocalizedAsync(survey, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(survey, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(survey, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(survey),
                DisplayCaptcha = _dynamicSurveySettings.CaptchaEnabled
            };

            //survey attributes
            model.SurveyAttributes = await PrepareSurveyAttributeModelsAsync(survey, attributesXml);

            return model;
        }

        protected virtual async Task<IList<SurveyModel.SurveyAttributeModel>> PrepareSurveyAttributeModelsAsync(Survey survey, string attributesXml = "")
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            var model = new List<SurveyModel.SurveyAttributeModel>();

            var surveyAttributeMapping = await _surveyAttributeService.GetSurveyAttributeMappingsBySurveyIdAsync(survey.Id);
            foreach (var attribute in surveyAttributeMapping)
            {
                var surveyAttrubute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(attribute.SurveyAttributeId);

                var attributeModel = new SurveyModel.SurveyAttributeModel
                {
                    Id = attribute.Id,
                    SurveyId = survey.Id,
                    SurveyAttributeId = attribute.SurveyAttributeId,
                    Name = await _localizationService.GetLocalizedAsync(surveyAttrubute, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(surveyAttrubute, x => x.Description),
                    TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };

                if (attribute.AttributeControlType == AttributeControlType.Datepicker)
                {
                    if (attribute.DefaultDate.HasValue)
                    {
                        attributeModel.SelectedDay = attribute.DefaultDate.Value.Day;
                        attributeModel.SelectedMonth = attribute.DefaultDate.Value.Month;
                        attributeModel.SelectedYear = attribute.DefaultDate.Value.Year;
                    }

                    attributeModel.ValidationMinDate = attribute.ValidationMinDate;
                    attributeModel.ValidationMaxDate = attribute.ValidationMaxDate;
                }

                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _surveyAttributeService.GetSurveyAttributeValuesAsync(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new SurveyModel.SurveyAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);

                        //"image square" picture (with with "image squares" attribute type only)
                        if (attributeValue.ImageSquaresPictureId > 0)
                        {
                            var surveyAttributeImageSquarePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(DynamicSurveyDefaults.SurveyAttributeImageSquarePictureModelKey
                                , attributeValue.ImageSquaresPictureId,
                                    _webHelper.IsCurrentConnectionSecured(),
                                    await _storeContext.GetCurrentStoreAsync());
                            valueModel.ImageSquaresPictureModel = await _staticCacheManager.GetAsync(surveyAttributeImageSquarePictureCacheKey, async () =>
                            {
                                var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(attributeValue.ImageSquaresPictureId);
                                string fullSizeImageUrl, imageUrl;
                                (imageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture, _dynamicSurveySettings.ImageSquarePictureSize);
                                (fullSizeImageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);

                                if (imageSquaresPicture != null)
                                {
                                    return new PictureModel
                                    {
                                        FullSizeImageUrl = fullSizeImageUrl,
                                        ImageUrl = imageUrl
                                    };
                                }

                                return new PictureModel();
                            });
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(attributesXml))
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                            //clear default selection                                
                            foreach (var item in attributeModel.Values)
                                item.IsPreSelected = false;

                            //select new values
                            var selectedValues = await _surveyAttributeParser.ParseSurveyAttributeValuesAsync(attributesXml);
                            foreach (var attributeValue in selectedValues)
                                foreach (var item in attributeModel.Values)
                                    if (attributeValue.Id == item.Id)
                                        item.IsPreSelected = true;
                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            attributeModel.DefaultValue = _surveyAttributeParser.ParseValues(attributesXml, attribute.Id).FirstOrDefault();
                            break;
                        case AttributeControlType.Datepicker:
                            var selectedDateStr = _surveyAttributeParser.ParseValues(attributesXml, attribute.Id).FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(selectedDateStr) && DateTime.TryParse(selectedDateStr, out var selectedDate))
                            {
                                attributeModel.SelectedDay = selectedDate.Day;
                                attributeModel.SelectedMonth = selectedDate.Month;
                                attributeModel.SelectedYear = selectedDate.Year;
                            }
                            break;
                        case AttributeControlType.FileUpload:
                            break;
                        case AttributeControlType.ColorSquares:
                            break;
                        case AttributeControlType.ImageSquares:
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            break;
                        default:
                            break;
                    }
                }

                model.Add(attributeModel);
            }

            return model;
        }

        #endregion

        #region Methods

        public async Task<SurveyTopMenuModel> PrepareSurveyTopMenuModelAsync(string widgetZone)
        {
            var surveys = await _surveyService.SearchSurveysAsync(
                storeId: _storeContext.GetCurrentStore().Id,
                includedInTopMenu: true,
                validScheduleOnly: true);

            var model = new SurveyTopMenuModel();
            model.WidgetZone = widgetZone;

            foreach (var survey in surveys)
            {
                model.Surveys.Add(new SurveyTopMenuModel.SurveyModel
                {
                    Id = survey.Id,
                    Name = await _localizationService.GetLocalizedAsync(survey, x => survey.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(survey)
                });
            }

            return model;
        }

        public async Task<IList<SurveyOverviewModel>> PrepareSurveyOverviewModelsAsync(string widgetZone)
        {
            var surveys = await _surveyService.SearchSurveysAsync(
                storeId: _storeContext.GetCurrentStore().Id,
                widgetZone: widgetZone,
                validScheduleOnly: true);

            var model = new List<SurveyOverviewModel>();

            foreach (var survey in surveys)
            {
                model.Add(new SurveyOverviewModel
                {
                    Id = survey.Id,
                    Name = await _localizationService.GetLocalizedAsync(survey, x => survey.Name),
                    SystemName = survey.SystemName,
                    SeName = await _urlRecordService.GetSeNameAsync(survey)
                });
            }

            return model;
        }

        public virtual async Task<SurveyModel> PrepareSurveyModelBySystemNameAsync(string systemName)
        {
            //load by store
            var store = await _storeContext.GetCurrentStoreAsync();
            var survey = await _surveyService.GetSurveyBySystemNameAsync(systemName, store.Id);
            if (survey == null)
                return null;

            return await PrepareSurveyModelAsync(survey);
        }

        public virtual async Task<SurveyModel> PrepareSurveyModelByIdAsync(Survey survey, string attributesXml = "")
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            return await PrepareSurveyModelAsync(survey, attributesXml);
        }

        #endregion
    }
}
