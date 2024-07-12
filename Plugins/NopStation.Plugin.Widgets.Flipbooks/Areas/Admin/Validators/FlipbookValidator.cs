using FluentValidation;
using NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Widgets.Flipbooks.Admin.Areas.Validators
{
    public class FlipbookValidator : BaseNopValidator<FlipbookModel>
    {
        #region Ctor

        public FlipbookValidator(ILocalizationService localizationService)
        {
            //set validation rules
            RuleFor(model => model.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Flipbooks.Fields.Name.Required").Result);
        }

        #endregion
    }
}