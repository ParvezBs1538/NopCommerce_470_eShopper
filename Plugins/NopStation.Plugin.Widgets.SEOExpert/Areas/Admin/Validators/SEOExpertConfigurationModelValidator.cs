using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Models;

namespace Nop.Plugin.DiscountRules.CustomerRoles.Validators
{
    /// <summary>
    /// Represents an <see cref="RequirementModel"/> validator.
    /// </summary>
    public class SEOExpertConfigurationModelValidator : BaseNopValidator<SEOExpertConfigurationModel>
    {
        public SEOExpertConfigurationModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.Temperature)
                .InclusiveBetween(0, 1)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.Temperature.RangeValidation"));

        }
    }
}
