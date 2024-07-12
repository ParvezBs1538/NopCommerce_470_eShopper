using FluentValidation;
using NopStation.Plugin.SMS.Messente.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.SMS.Messente.Areas.Admin.Validators
{
    public class SmsTemplateValidator : BaseNopValidator<SmsTemplateModel>
    {
        public SmsTemplateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Body).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Messente.SmsTemplates.Fields.Body.Required").Result);
        }
    }
}
