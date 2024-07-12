using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Validators
{
    public class AjaxFilterConfigureValidator : BaseNopValidator<ConfigurationModel>
    {
        public AjaxFilterConfigureValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.MaxDisplayForCategories).GreaterThan(-1).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.NotPositive"));
            RuleFor(x => x.MaxDisplayForManufacturers).GreaterThan(-1).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.NotPositive"));
        }
    }
}
