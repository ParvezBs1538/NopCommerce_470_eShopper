using FluentValidation;
using NopStation.Plugin.Misc.IpFilter.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Misc.IpFilter.Validators
{
    public class IpRangeBlockRuleValidator : BaseNopValidator<IpRangeBlockRuleModel>
    {
        public IpRangeBlockRuleValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FromIpAddress)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.FromIpAddress.Required").Result);
            RuleFor(x => x.ToIpAddress)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.ToIpAddress.Required").Result);
            RuleFor(x => x.FromIpAddress)
                .IsIpAddress()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.IpFilter.InvalidIpAddress").Result);
            RuleFor(x => x.ToIpAddress)
                .IsIpAddress()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.IpFilter.InvalidIpAddress").Result);
        }
    }
}
