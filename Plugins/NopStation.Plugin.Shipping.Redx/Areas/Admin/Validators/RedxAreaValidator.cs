using FluentValidation;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Validators
{
    public class RedxAreaValidator : BaseNopValidator<RedxAreaModel>
    {
        public RedxAreaValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Redx.RedxAreas.Fields.Name.Required").Result);
            RuleFor(x => x.PostCode).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Redx.RedxAreas.Fields.PostCode.Required").Result);
        }
    }
}