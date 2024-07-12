using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.DiscountRules.ShippingCountry.Models;

namespace NopStation.Plugin.DiscountRules.ShippingCountry.Validators
{
    public class ConfigurationValidator : BaseNopValidator<RequirementModel>
    {
        #region Ctor
        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.ShippingCountry)
               .NotEqual("0")
               .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.ShippingCountry.Fields.ShippingCountry.Required"));
        }

        #endregion
    }
}