using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.Core.Models;

namespace NopStation.Plugin.Misc.Core.Validators;

public class LocaleResourceValidator : BaseNopValidator<CoreLocaleResourceModel>
{
    public LocaleResourceValidator()
    {
        RuleFor(x => x.ResourceName).NotEmpty().WithMessage("Admin.Configuration.Languages.Resources.Fields.Name.Required");
    }
}
