using System;
using System.Collections.Generic;
using System.Net;
using Firewall;
using Microsoft.AspNetCore.Http;
using Nop.Core;

namespace NopStation.Plugin.Misc.IpFilter.Helper
{
    public sealed class IPAddressRangeRule : IFirewallRule
    {
        private readonly IFirewallRule _nextRule;
        private readonly IList<CIDRNotation> _cidrNotations;
        private readonly IWebHelper _webHelper;

        public IPAddressRangeRule(IFirewallRule nextRule, 
            IList<CIDRNotation> cidrNotations, 
            IWebHelper webHelper)
        {
            _nextRule = nextRule;
            _cidrNotations = cidrNotations;
            _webHelper = webHelper;
        }

        public bool IsAllowed(HttpContext context)
        {
            // get IP address
            var remoteIpAddress = _webHelper.GetCurrentIpAddress();
            // check weather it exits in block list or not
            var (isExits, cidr) = MatchesAnyIPAddressRange(IPAddress.Parse(remoteIpAddress));

            if (isExits)
            {
                return false;
            }

            return isExits || _nextRule.IsAllowed(context);
        }

        // check IP address if exits or not in block list
        private (bool, CIDRNotation) MatchesAnyIPAddressRange(IPAddress remoteIpAddress)
        {
            if (_cidrNotations != null && _cidrNotations.Count > 0)
                foreach (var cidr in _cidrNotations)
                    if (cidr.Contains(remoteIpAddress))
                        return (true, cidr);

            return (false, null);
        }
    }
}
