using System.Collections.Generic;
using System.Net;
using Firewall;
using Nop.Core;
using Nop.Services.Directory;

namespace NopStation.Plugin.Misc.IpFilter.Helper
{
    public static class FirewallRulesEngineExtensions
    {
        // IP address denied rule
        public static IFirewallRule DenyIpAddresses(this IFirewallRule rule, IList<IPAddress> notAllowedIPAddress, IWebHelper webHelper)
        {
            return new IPAddressRule(rule, notAllowedIPAddress, webHelper);
        }

        // country deny rules
        public static IFirewallRule DenyCountries(this IFirewallRule rule, IList<string> denyCountryCode, IWebHelper webHelper, IGeoLookupService geoLookupService)
        {
            return new IpCountryRule(rule, denyCountryCode, webHelper, geoLookupService);
        }

        // IP range deny rules 
        public static IFirewallRule DenyIPAddressRange(this IFirewallRule rule, IList<CIDRNotation> notAllowCidrNotations, IWebHelper webHelper)
        {
            return new IPAddressRangeRule(rule, notAllowCidrNotations, webHelper);
        }

        // all access rules
        public static IFirewallRule AccessAllRules(this IFirewallRule rule)
        {
            return new AccessAllRule(rule);
        }
    }
}
