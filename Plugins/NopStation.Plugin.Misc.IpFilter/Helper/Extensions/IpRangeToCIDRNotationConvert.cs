using System;
using System.Collections.Generic;
using System.Net;

namespace NopStation.Plugin.Misc.IpFilter.Helper.Extensions
{
    public class IPRangeToCIDRNotationConvert
    {
        public List<string> IPRange2CIDRNotation(string ipStart, string ipEnd)
        {
            var start = IP2long(ipStart);
            var end = IP2long(ipEnd);
            var result = new List<string>();

            while (end >= start)
            {
                byte maxSize = 32;
                while (maxSize > 0)
                {
                    var mask = IMask(maxSize - 1);
                    var maskBase = start & mask;

                    if (maskBase != start)
                    {
                        break;
                    }

                    maxSize--;
                }
                var x = Math.Log(end - start + 1) / Math.Log(2);
                var maxDiff = (byte)(32 - Math.Floor(x));
                if (maxSize < maxDiff)
                {
                    maxSize = maxDiff;
                }
                var ip = Long2ip(start);
                result.Add(ip + "/" + maxSize);
                start += (long)Math.Pow(2, (32 - maxSize));
            }
            return result;
        }

        private long IMask(int s)
        {
            return (long)(Math.Pow(2, 32) - Math.Pow(2, (32 - s)));
        }

        private string Long2ip(long ipAddress)
        {
            IPAddress ip;
            if (IPAddress.TryParse(ipAddress.ToString(), out ip))
            {
                return ip.ToString();
            }
            return "error";
        }

        private long IP2long(string ipAddress)
        {
            IPAddress ip;
            if (IPAddress.TryParse(ipAddress, out ip))
            {
                return (((long)ip.GetAddressBytes()[0] << 24) | ((long)ip.GetAddressBytes()[1] << 16) | ((long)ip.GetAddressBytes()[2] << 8) | ip.GetAddressBytes()[3]);
            }
            return -1;
        }
    }
}
