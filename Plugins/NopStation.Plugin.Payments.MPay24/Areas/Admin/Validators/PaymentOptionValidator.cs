using FluentValidation;
using NopStation.Plugin.Payments.MPay24.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Payments.MPay24.Areas.Admin.Validators
{
    public class PaymentOptionValidator : BaseNopValidator<PaymentOptionModel>
    {
        public PaymentOptionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.PaymentType).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.MPay24.PaymentOptions.Fields.PaymentType.Required").Result);
            RuleFor(x => x.DisplayName).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.MPay24.PaymentOptions.Fields.DisplayName.Required").Result);
            RuleFor(x => x.Brand).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.MPay24.PaymentOptions.Fields.Brand.Required").Result);
            RuleFor(x => x.ShortName).NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.MPay24.PaymentOptions.Fields.ShortName.Required").Result);
        }
    }
}
