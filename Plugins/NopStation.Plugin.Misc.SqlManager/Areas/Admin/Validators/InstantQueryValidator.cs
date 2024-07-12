using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Validators
{
    public class InstantQueryValidator : BaseNopValidator<SqlQueryModel>
    {
        public InstantQueryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.SQLQuery)
               .NotEmpty()
               .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlReports.Fields.Query.Required").Result);
        }
    }
}
