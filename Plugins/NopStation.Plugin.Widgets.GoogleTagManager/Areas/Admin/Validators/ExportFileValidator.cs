using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.GoogleTagManager.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.GoogleTagManager.Areas.Admin.Validators
{
    public partial class ExportFileValidator : BaseNopValidator<FileInformationModel>
    {
        public ExportFileValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.GAContainerId).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.GoogleTagManager.Configuration.GTMContainerId.Required").Result);
        }
    }
}
