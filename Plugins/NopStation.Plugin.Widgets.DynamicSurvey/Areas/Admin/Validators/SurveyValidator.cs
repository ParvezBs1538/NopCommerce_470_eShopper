using FluentValidation;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Validators
{
    public partial class SurveyValidator : BaseNopValidator<SurveyModel>
    {
        public SurveyValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Surveys.Fields.Name.Required"));

            RuleFor(x => x.SeName)
                .Length(0, NopSeoDefaults.SearchEngineNameLength)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.SEO.SeName.MaxLengthValidation"), NopSeoDefaults.SearchEngineNameLength);

            SetDatabaseValidationRules<Survey>();
        }
    }
}