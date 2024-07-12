using Firewall;
using Microsoft.AspNetCore.Http;

namespace NopStation.Plugin.Misc.IpFilter.Helper
{
    public sealed class AccessAllRule : IFirewallRule
    {
        private readonly IFirewallRule _rule;

        public AccessAllRule(IFirewallRule rule)
        {
            _rule = rule;
        }

        // allow for all users
        public bool IsAllowed(HttpContext context)
        {
            return true;
        }
    }
}
