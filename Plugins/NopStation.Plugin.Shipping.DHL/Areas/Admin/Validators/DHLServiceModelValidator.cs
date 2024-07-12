using FluentValidation;
using NopStation.Plugin.Shipping.DHL.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Validators
{
    public class DHLServiceModelValidator : BaseNopValidator<DHLServiceModel>
    {
        public DHLServiceModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ServiceName).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.DHL.Services.Fields.ServiceName.Required").Result);
            RuleFor(x => x.GlobalProductCode).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.DHL.Services.Fields.GlobalProductCode.Required").Result);
            RuleFor(x => x.IsActive).Equal(true)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.DHL.Services.Fields.IsActive.Required").Result);
        }
    }
}
