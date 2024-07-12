using FluentValidation;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Validators
{
    public partial class SurveyAttributeValidator : BaseNopValidator<SurveyAttributeModel>
    {
        public SurveyAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Name.Required"));

            SetDatabaseValidationRules<SurveyAttribute>();
        }
    }
}