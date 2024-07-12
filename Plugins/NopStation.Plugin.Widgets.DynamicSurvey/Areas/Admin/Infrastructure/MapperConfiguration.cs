using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public MapperConfiguration()
        {
            CreateMap<DynamicSurveySettings, ConfigurationModel>()
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, DynamicSurveySettings>();

            CreateMap<Survey, SurveyModel>()
                .ForMember(model => model.CopySurveyModel, options => options.Ignore())
                .ForMember(model => model.SurveyAttributeMappingSearchModel, options => options.Ignore())
                .ForMember(model => model.SurveyAttributesExist, options => options.Ignore())
                .ForMember(model => model.SeName, options => options.Ignore());
            CreateMap<SurveyModel, Survey>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.Deleted, options => options.Ignore());

            CreateMap<PredefinedSurveyAttributeValue, PredefinedSurveyAttributeValueModel>()
                .ForMember(model => model.Locales, options => options.Ignore());
            CreateMap<PredefinedSurveyAttributeValueModel, PredefinedSurveyAttributeValue>();

            CreateMap<SurveyAttribute, SurveyAttributeModel>()
                .ForMember(model => model.SurveyAttributeSurveySearchModel, options => options.Ignore());
            CreateMap<SurveyAttributeModel, SurveyAttribute>();

            CreateMap<Survey, SurveyAttributeSurveyModel>()
                .ForMember(model => model.SurveyName, options => options.Ignore());

            CreateMap<SurveyAttributeMapping, SurveyAttributeMappingModel>()
                .ForMember(model => model.ValidationRulesString, options => options.Ignore())
                .ForMember(model => model.AttributeControlType, options => options.Ignore())
                .ForMember(model => model.ConditionString, options => options.Ignore())
                .ForMember(model => model.SurveyAttribute, options => options.Ignore())
                .ForMember(model => model.AvailableSurveyAttributes, options => options.Ignore())
                .ForMember(model => model.ConditionAllowed, options => options.Ignore())
                .ForMember(model => model.ConditionModel, options => options.Ignore())
                .ForMember(model => model.SurveyAttributeValueSearchModel, options => options.Ignore());
            CreateMap<SurveyAttributeMappingModel, SurveyAttributeMapping>()
                .ForMember(entity => entity.ConditionAttributeXml, options => options.Ignore())
                .ForMember(entity => entity.AttributeControlType, options => options.Ignore());

            CreateMap<SurveyAttributeValue, SurveyAttributeValueModel>()
                .ForMember(model => model.Name, options => options.Ignore())
                .ForMember(model => model.DisplayColorSquaresRgb, options => options.Ignore())
                .ForMember(model => model.DisplayImageSquaresPicture, options => options.Ignore());
            CreateMap<SurveyAttributeValueModel, SurveyAttributeValue>();

            CreateMap<SurveySubmission, SurveySubmissionModel>()
                .ForMember(model => model.CustomerEmail, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore());
            CreateMap<SurveySubmissionModel, SurveySubmission>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());
        }

        #endregion

        #region Properties

        public int Order => 0;

        #endregion
    }
}