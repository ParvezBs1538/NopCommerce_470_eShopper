using System.Net;
using FluentValidation;

namespace NopStation.Plugin.Misc.IpFilter.Validators
{
    public static class CustomValidators
    {
        public static IRuleBuilderOptions<T, string> IsIpAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(m => m != null && IPAddress.TryParse(m, out _));
        }
    }
}
