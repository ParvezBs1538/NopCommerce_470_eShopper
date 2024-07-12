using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Validators
{
    public class DepartmentValidator : BaseNopValidator<DepartmentModel>
    {
        public DepartmentValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Departments.Fields.Name.Required").Result);
        }
    }
}
