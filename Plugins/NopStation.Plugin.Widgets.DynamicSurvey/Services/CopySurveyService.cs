using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public partial class CopySurveyService : ICopySurveyService
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPictureService _pictureService;
        private readonly ISurveyAttributeParser _surveyAttributeParser;
        private readonly ISurveyAttributeService _surveyAttributeService;
        private readonly ISurveyService _surveyService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public CopySurveyService(ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IPictureService pictureService,
            ISurveyAttributeParser surveyAttributeParser,
            ISurveyAttributeService surveyAttributeService,
            ISurveyService surveyService,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _pictureService = pictureService;
            _pictureService = pictureService;
            _surveyAttributeParser = surveyAttributeParser;
            _surveyAttributeService = surveyAttributeService;
            _surveyService = surveyService;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        protected virtual async Task CopyAttributesMappingAsync(Survey survey, Survey surveyCopy)
        {
            var associatedAttributes = new Dictionary<int, int>();
            var associatedAttributeValues = new Dictionary<int, int>();

            //attribute mapping with condition attributes
            var oldCopyWithConditionAttributes = new List<SurveyAttributeMapping>();

            //all survey attribute mapping copies
            var surveyAttributeMappingCopies = new Dictionary<int, SurveyAttributeMapping>();

            var languages = await _languageService.GetAllLanguagesAsync(true);

            foreach (var surveyAttributeMapping in await _surveyAttributeService.GetSurveyAttributeMappingsBySurveyIdAsync(survey.Id))
            {
                var surveyAttributeMappingCopy = new SurveyAttributeMapping
                {
                    SurveyId = surveyCopy.Id,
                    SurveyAttributeId = surveyAttributeMapping.SurveyAttributeId,
                    TextPrompt = surveyAttributeMapping.TextPrompt,
                    IsRequired = surveyAttributeMapping.IsRequired,
                    AttributeControlTypeId = surveyAttributeMapping.AttributeControlTypeId,
                    DisplayOrder = surveyAttributeMapping.DisplayOrder,
                    ValidationMinLength = surveyAttributeMapping.ValidationMinLength,
                    ValidationMaxLength = surveyAttributeMapping.ValidationMaxLength,
                    ValidationFileAllowedExtensions = surveyAttributeMapping.ValidationFileAllowedExtensions,
                    ValidationFileMaximumSize = surveyAttributeMapping.ValidationFileMaximumSize,
                    DefaultValue = surveyAttributeMapping.DefaultValue
                };
                await _surveyAttributeService.InsertSurveyAttributeMappingAsync(surveyAttributeMappingCopy);
                //localization
                foreach (var lang in languages)
                {
                    var textPrompt = await _localizationService.GetLocalizedAsync(surveyAttributeMapping, x => x.TextPrompt, lang.Id, false, false);
                    if (!string.IsNullOrEmpty(textPrompt))
                        await _localizedEntityService.SaveLocalizedValueAsync(surveyAttributeMappingCopy, x => x.TextPrompt, textPrompt,
                            lang.Id);
                }

                surveyAttributeMappingCopies.Add(surveyAttributeMappingCopy.Id, surveyAttributeMappingCopy);

                if (!string.IsNullOrEmpty(surveyAttributeMapping.ConditionAttributeXml))
                {
                    oldCopyWithConditionAttributes.Add(surveyAttributeMapping);
                }

                //save associated value (used for combinations copying)
                associatedAttributes.Add(surveyAttributeMapping.Id, surveyAttributeMappingCopy.Id);

                // survey attribute values
                var surveyAttributeValues = await _surveyAttributeService.GetSurveyAttributeValuesAsync(surveyAttributeMapping.Id);
                foreach (var surveyAttributeValue in surveyAttributeValues)
                {
                    var attributeValueCopy = new SurveyAttributeValue
                    {
                        SurveyAttributeMappingId = surveyAttributeMappingCopy.Id,
                        Name = surveyAttributeValue.Name,
                        ColorSquaresRgb = surveyAttributeValue.ColorSquaresRgb,
                        IsPreSelected = surveyAttributeValue.IsPreSelected,
                        DisplayOrder = surveyAttributeValue.DisplayOrder
                    };
                    //picture associated to "iamge square" attribute type (if exists)
                    if (surveyAttributeValue.ImageSquaresPictureId > 0)
                    {
                        var origImageSquaresPicture =
                            await _pictureService.GetPictureByIdAsync(surveyAttributeValue.ImageSquaresPictureId);
                        if (origImageSquaresPicture != null)
                        {
                            //copy the picture
                            var imageSquaresPictureCopy = await _pictureService.InsertPictureAsync(
                                await _pictureService.LoadPictureBinaryAsync(origImageSquaresPicture),
                                origImageSquaresPicture.MimeType,
                                origImageSquaresPicture.SeoFilename,
                                origImageSquaresPicture.AltAttribute,
                                origImageSquaresPicture.TitleAttribute);
                            attributeValueCopy.ImageSquaresPictureId = imageSquaresPictureCopy.Id;
                        }
                    }

                    await _surveyAttributeService.InsertSurveyAttributeValueAsync(attributeValueCopy);

                    //save associated value (used for combinations copying)
                    associatedAttributeValues.Add(surveyAttributeValue.Id, attributeValueCopy.Id);

                    //localization
                    foreach (var lang in languages)
                    {
                        var name = await _localizationService.GetLocalizedAsync(surveyAttributeValue, x => x.Name, lang.Id, false, false);
                        if (!string.IsNullOrEmpty(name))
                            await _localizedEntityService.SaveLocalizedValueAsync(attributeValueCopy, x => x.Name, name, lang.Id);
                    }
                }
            }

            //copy attribute conditions
            foreach (var surveyAttributeMapping in oldCopyWithConditionAttributes)
            {
                var oldConditionAttributeMapping = (await _surveyAttributeParser
                    .ParseSurveyAttributeMappingsAsync(surveyAttributeMapping.ConditionAttributeXml)).FirstOrDefault();

                if (oldConditionAttributeMapping == null)
                    continue;

                var oldConditionValues = await _surveyAttributeParser.ParseSurveyAttributeValuesAsync(
                    surveyAttributeMapping.ConditionAttributeXml,
                    oldConditionAttributeMapping.Id);

                if (!oldConditionValues.Any())
                    continue;

                var newAttributeMappingId = associatedAttributes[oldConditionAttributeMapping.Id];
                var newConditionAttributeMapping = surveyAttributeMappingCopies[newAttributeMappingId];

                var newConditionAttributeXml = string.Empty;

                foreach (var oldConditionValue in oldConditionValues)
                {
                    newConditionAttributeXml = _surveyAttributeParser.AddSurveyAttribute(newConditionAttributeXml,
                        newConditionAttributeMapping, associatedAttributeValues[oldConditionValue.Id].ToString());
                }

                var attributeMappingId = associatedAttributes[surveyAttributeMapping.Id];
                var conditionAttribute = surveyAttributeMappingCopies[attributeMappingId];
                conditionAttribute.ConditionAttributeXml = newConditionAttributeXml;

                await _surveyAttributeService.UpdateSurveyAttributeMappingAsync(conditionAttribute);
            }
        }

        protected virtual async Task CopyLocalizationDataAsync(Survey survey, Survey surveyCopy)
        {
            var languages = await _languageService.GetAllLanguagesAsync(true);

            //localization
            foreach (var lang in languages)
            {
                var name = await _localizationService.GetLocalizedAsync(survey, x => x.Name, lang.Id, false, false);
                if (!string.IsNullOrEmpty(name))
                    await _localizedEntityService.SaveLocalizedValueAsync(surveyCopy, x => x.Name, name, lang.Id);

                var shortDescription = await _localizationService.GetLocalizedAsync(survey, x => x.Description, lang.Id, false, false);
                if (!string.IsNullOrEmpty(shortDescription))
                    await _localizedEntityService.SaveLocalizedValueAsync(surveyCopy, x => x.Description, shortDescription, lang.Id);

                var metaKeywords = await _localizationService.GetLocalizedAsync(survey, x => x.MetaKeywords, lang.Id, false, false);
                if (!string.IsNullOrEmpty(metaKeywords))
                    await _localizedEntityService.SaveLocalizedValueAsync(surveyCopy, x => x.MetaKeywords, metaKeywords, lang.Id);

                var metaDescription = await _localizationService.GetLocalizedAsync(survey, x => x.MetaDescription, lang.Id, false, false);
                if (!string.IsNullOrEmpty(metaDescription))
                    await _localizedEntityService.SaveLocalizedValueAsync(surveyCopy, x => x.MetaDescription, metaDescription, lang.Id);

                var metaTitle = await _localizationService.GetLocalizedAsync(survey, x => x.MetaTitle, lang.Id, false, false);
                if (!string.IsNullOrEmpty(metaTitle))
                    await _localizedEntityService.SaveLocalizedValueAsync(surveyCopy, x => x.MetaTitle, metaTitle, lang.Id);

                //search engine name
                await _urlRecordService.SaveSlugAsync(surveyCopy, await _urlRecordService.ValidateSeNameAsync(surveyCopy, string.Empty, name, false), lang.Id);
            }
        }

        protected virtual async Task<Survey> CopyBaseSurveyDataAsync(Survey survey, string newName, bool isPublished)
        {
            // survey
            var surveyCopy = new Survey
            {
                Name = newName,
                MetaKeywords = survey.MetaKeywords,
                MetaDescription = survey.MetaDescription,
                MetaTitle = survey.MetaTitle,
                LimitedToStores = survey.LimitedToStores,
                DisplayOrder = survey.DisplayOrder,
                Published = isPublished,
                Deleted = survey.Deleted,
                CreatedOnUtc = DateTime.UtcNow,
                AllowMultipleSubmissions = survey.AllowMultipleSubmissions,
                SubjectToAcl = survey.SubjectToAcl,
                EndDateUtc = survey.EndDateUtc,
                Description = survey.Description,
                StartDateUtc = survey.StartDateUtc,
                IncludeInTopMenu = survey.IncludeInTopMenu
            };

            //validate search engine name
            await _surveyService.InsertSurveyAsync(surveyCopy);

            //search engine name
            await _urlRecordService.SaveSlugAsync(surveyCopy, await _urlRecordService.ValidateSeNameAsync(surveyCopy, string.Empty, surveyCopy.Name, true), 0);
            return surveyCopy;
        }

        #endregion

        #region Methods

        public virtual async Task<Survey> CopySurveyAsync(Survey survey, string newName, bool isPublished = true)
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("Survey name is required");

            var surveyCopy = await CopyBaseSurveyDataAsync(survey, newName, isPublished);

            //localization
            await CopyLocalizationDataAsync(survey, surveyCopy);

            await _surveyService.UpdateSurveyAsync(surveyCopy);

            //survey <-> attributes mappings
            await CopyAttributesMappingAsync(survey, surveyCopy);
            //store mapping
            var selectedStoreIds = await _storeMappingService.GetStoresIdsWithAccessAsync(survey);
            foreach (var id in selectedStoreIds)
                await _storeMappingService.InsertStoreMappingAsync(surveyCopy, id);

            return surveyCopy;
        }

        #endregion
    }
}
