using System;
using FluentValidation;
using NopStation.Plugin.Misc.IpFilter.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Misc.IpFilter.Validators
{
    public class IpBlockRuleValidator : BaseNopValidator<IpBlockRuleModel>
    {
        public IpBlockRuleValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.IpAddress)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.IpFilter.IpBlockRules.Fields.IpAddress.Required").Result);

            RuleFor(x => x.IpAddress)
                .IsIpAddress()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.IpFilter.InvalidIpAddress").Result);
        }
    }
}
