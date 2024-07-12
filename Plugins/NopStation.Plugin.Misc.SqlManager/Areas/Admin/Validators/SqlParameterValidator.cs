using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Validators
{
    public class SqlParameterValidator : BaseNopValidator<SqlParameterModel>
    {
        public SqlParameterValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlParameters.Fields.Name.Required").Result);
            RuleFor(x => x.SystemName).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlParameters.Fields.SystemName.Required").Result);
        }
    }
}
