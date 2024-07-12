using FluentValidation;
using NopStation.Plugin.Misc.IpFilter.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Misc.IpFilter.Validators
{
    public class CountryBlockRuleValidator : BaseNopValidator<CountryBlockRuleModel>
    {
        public CountryBlockRuleValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.CountryId)
                .GreaterThan(0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.IpFilter.CountryBlockRules.Fields.Country.Required").Result);
        }
    }
}
