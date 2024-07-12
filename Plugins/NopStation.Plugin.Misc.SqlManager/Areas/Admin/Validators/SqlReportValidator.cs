using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Validators
{
    public class SqlReportValidator : BaseNopValidator<SqlReportModel>
    {
        public SqlReportValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlReports.Fields.Name.Required").Result);
            RuleFor(x => x.Query).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlReports.Fields.Query.Required").Result);
        }
    }
}
