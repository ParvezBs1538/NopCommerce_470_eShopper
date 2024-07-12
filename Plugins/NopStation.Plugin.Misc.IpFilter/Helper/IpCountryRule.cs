using System.Collections.Generic;
using Firewall;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Services.Directory;

namespace NopStation.Plugin.Misc.IpFilter.Helper
{
    public sealed class IpCountryRule : IFirewallRule
    {
        private readonly IFirewallRule _nextRule;
        private readonly IList<string> _denyCountryCodes;
        private readonly IWebHelper _webHelper;
        private readonly IGeoLookupService _geoLookupService;

        public IpCountryRule(IFirewallRule rule, 
            IList<string> denyCountryCodes, 
            IWebHelper webHelper, 
            IGeoLookupService geoLookupService)
        {
            _nextRule = rule;
            _denyCountryCodes = denyCountryCodes;
            _webHelper = webHelper;
            _geoLookupService = geoLookupService;
        }

        public bool IsAllowed(HttpContext context)
        {
            try
            {
                // get current IP Address
                var remoteIpAddress = _webHelper.GetCurrentIpAddress();

                // using geoLocation for country code
                var countryISOCode = _geoLookupService.LookupCountryIsoCode(remoteIpAddress);

                // check weather code contain in blocklist or not
                var isExits = _denyCountryCodes.Contains(countryISOCode);

                if (isExits)
                    return false;

                return isExits || _nextRule.IsAllowed(context);
            }
            catch (AddressNotFoundException)
            {
                return _nextRule.IsAllowed(context);
            }
        }
    }
}
