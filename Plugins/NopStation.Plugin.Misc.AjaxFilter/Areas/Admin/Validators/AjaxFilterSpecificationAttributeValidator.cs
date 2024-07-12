using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Validators
{
    public class AjaxFilterSpecificationAttributeValidator : BaseNopValidator<AjaxFilterSpecificationAttributeModel>
    {
        public AjaxFilterSpecificationAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.MaxSpecificationAttributesToDisplay).GreaterThan(-1).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.NotPositive"));
        }
    }
}
