using System.Net;

namespace NopStation.Plugin.Misc.IpFilter.Helper.Extensions
{
    public static class IPAddressExtensions
    {
        public static byte[] GetMappedAddressBytes(this IPAddress address)
        {
            return address.IsIPv4MappedToIPv6 ? address.MapToIPv4().GetAddressBytes() : address.GetAddressBytes();
        }

        public static bool IsEqualTo(this IPAddress address, IPAddress otherAddress)
        {
            return address.GetAddressBytes().IsEqualTo(otherAddress.GetMappedAddressBytes());
        }
    }
}
