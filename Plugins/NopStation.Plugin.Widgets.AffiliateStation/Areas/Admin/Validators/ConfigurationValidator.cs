using FluentValidation;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Validators
{
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.AffiliatePageOrderPageSize).GreaterThan(0);
        }
    }
}
