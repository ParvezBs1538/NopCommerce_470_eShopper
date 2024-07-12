using System;
using System.Collections.Generic;
using System.Net;
using Firewall;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using NopStation.Plugin.Misc.IpFilter.Helper.Extensions;

namespace NopStation.Plugin.Misc.IpFilter.Helper
{
    public sealed class IPAddressRule : IFirewallRule
    {
        // intialization
        private readonly IFirewallRule _nextRule;
        private readonly IList<IPAddress> _ipAddresses;
        private readonly IWebHelper _webHelper;

        // constructor for IP Address rules
        public IPAddressRule(IFirewallRule nextRule, IList<IPAddress> ipAddresses, IWebHelper webHelper)
        {
            _nextRule = nextRule ?? throw new ArgumentNullException(nameof(nextRule));
            _ipAddresses = ipAddresses ?? throw new ArgumentNullException(nameof(ipAddresses));
            _webHelper = webHelper;
        }

        // this function make decision if it is allow IP Address or not
        public bool IsAllowed(HttpContext context)
        {
            //get current IP Address
            var remoteIpAddress = _webHelper.GetCurrentIpAddress();

            // check weather IP is in blocklist or not
            var (isExits, ip) = MatchesAnyIPAddress(IPAddress.Parse(remoteIpAddress));

            // no access this IP Address
            if (isExits)
            {
                return false;
            }
            return isExits || _nextRule.IsAllowed(context);
        }

        // check if the current IP Address Exist or not in the block list
        private (bool, IPAddress) MatchesAnyIPAddress(IPAddress remoteIpAddress)
        {
            if (_ipAddresses != null && _ipAddresses.Count > 0)
                foreach (var ip in _ipAddresses)
                    if (ip.IsEqualTo(remoteIpAddress))
                        return (true, ip);

            return (false, null);
        }
    }
}
