using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Validators
{
    public partial class PredefinedSurveyAttributeValueModelValidator : BaseNopValidator<PredefinedSurveyAttributeValueModel>
    {
        public PredefinedSurveyAttributeValueModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.Name.Required"));
        }
    }
}