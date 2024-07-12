using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;
using NopStation.Plugin.Widgets.DynamicSurvey.Services;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Factories
{
    public partial class SurveyAttributeModelFactory : ISurveyAttributeModelFactory
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ISurveyAttributeService _surveyAttributeService;
        private readonly ISurveyService _surveyService;

        #endregion

        #region Ctor

        public SurveyAttributeModelFactory(ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            ISurveyAttributeService surveyAttributeService,
            ISurveyService surveyService)
        {
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _surveyAttributeService = surveyAttributeService;
            _surveyService = surveyService;
        }

        #endregion

        #region Utilities

        protected virtual PredefinedSurveyAttributeValueSearchModel PreparePredefinedSurveyAttributeValueSearchModel(
            PredefinedSurveyAttributeValueSearchModel searchModel, SurveyAttribute surveyAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (surveyAttribute == null)
                throw new ArgumentNullException(nameof(surveyAttribute));

            searchModel.SurveyAttributeId = surveyAttribute.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual SurveyAttributeSurveySearchModel PrepareSurveyAttributeSurveySearchModel(SurveyAttributeSurveySearchModel searchModel,
           SurveyAttribute surveyAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (surveyAttribute == null)
                throw new ArgumentNullException(nameof(surveyAttribute));

            searchModel.SurveyAttributeId = surveyAttribute.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        public virtual Task<SurveyAttributeSearchModel> PrepareSurveyAttributeSearchModelAsync(SurveyAttributeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<SurveyAttributeListModel> PrepareSurveyAttributeListModelAsync(SurveyAttributeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get survey attributes
            var surveyAttributes = await _surveyAttributeService
                .GetAllSurveyAttributesAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new SurveyAttributeListModel().PrepareToGrid(searchModel, surveyAttributes, () =>
            {
                //fill in model values from the entity
                return surveyAttributes.Select(attribute => attribute.ToModel<SurveyAttributeModel>());
            });

            return model;
        }

        public virtual async Task<SurveyAttributeModel> PrepareSurveyAttributeModelAsync(SurveyAttributeModel model,
            SurveyAttribute surveyAttribute, bool excludeProperties = false)
        {
            Func<SurveyAttributeLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (surveyAttribute != null)
            {
                //fill in model values from the entity
                model ??= surveyAttribute.ToModel<SurveyAttributeModel>();

                PreparePredefinedSurveyAttributeValueSearchModel(model.PredefinedSurveyAttributeValueSearchModel, surveyAttribute);
                PrepareSurveyAttributeSurveySearchModel(model.SurveyAttributeSurveySearchModel, surveyAttribute);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(surveyAttribute, entity => entity.Name, languageId, false, false);
                    locale.Description = await _localizationService.GetLocalizedAsync(surveyAttribute, entity => entity.Description, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }

        public virtual async Task<PredefinedSurveyAttributeValueListModel> PreparePredefinedSurveyAttributeValueListModelAsync(
           PredefinedSurveyAttributeValueSearchModel searchModel, SurveyAttribute surveyAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (surveyAttribute == null)
                throw new ArgumentNullException(nameof(surveyAttribute));

            //get predefined survey attribute values
            var values = (await _surveyAttributeService.GetPredefinedSurveyAttributeValuesAsync(surveyAttribute.Id)).ToPagedList(searchModel);

            //prepare list model
            var model = new PredefinedSurveyAttributeValueListModel().PrepareToGrid(searchModel, values, () =>
            {
                return values.Select(value =>
                {
                    //fill in model values from the entity
                    var predefinedSurveyAttributeValueModel = value.ToModel<PredefinedSurveyAttributeValueModel>();

                    return predefinedSurveyAttributeValueModel;
                });
            });

            return model;
        }

        public virtual async Task<PredefinedSurveyAttributeValueModel> PreparePredefinedSurveyAttributeValueModelAsync(PredefinedSurveyAttributeValueModel model,
            SurveyAttribute surveyAttribute, PredefinedSurveyAttributeValue surveyAttributeValue, bool excludeProperties = false)
        {
            if (surveyAttribute == null)
                throw new ArgumentNullException(nameof(surveyAttribute));

            Func<PredefinedSurveyAttributeValueLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (surveyAttributeValue != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = surveyAttributeValue.ToModel<PredefinedSurveyAttributeValueModel>();
                }

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(surveyAttributeValue, entity => entity.Name, languageId, false, false);
                };
            }

            model.SurveyAttributeId = surveyAttribute.Id;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }

        public virtual async Task<SurveyAttributeSurveyListModel> PrepareSurveyAttributeSurveyListModelAsync(SurveyAttributeSurveySearchModel searchModel,
            SurveyAttribute surveyAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (surveyAttribute == null)
                throw new ArgumentNullException(nameof(surveyAttribute));

            //get surveys
            var surveys = await _surveyService.GetSurveysBySurveyAttributeIdAsync(surveyAttributeId: surveyAttribute.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new SurveyAttributeSurveyListModel().PrepareToGrid(searchModel, surveys, () =>
            {
                //fill in model values from the entity
                return surveys.Select(survey =>
                {
                    var surveyAttributeSurveyModel = survey.ToModel<SurveyAttributeSurveyModel>();
                    surveyAttributeSurveyModel.SurveyName = survey.Name;
                    return surveyAttributeSurveyModel;
                });
            });

            return model;
        }

        #endregion
    }
}