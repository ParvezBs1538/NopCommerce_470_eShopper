namespace NopStation.Plugin.Misc.IpFilter.Helper.Extensions
{
    public static class ByteArrayExtensions
    {
        public static bool IsEqualTo(this byte[] bytes, byte[] otherBytes)
        {
            if (bytes == null && otherBytes == null)
                return true;
            if (bytes == null)
                return false;
            if (otherBytes == null)
                return false;
            if (bytes.Length != otherBytes.Length)
                return false;

            for (var i = 0; i < bytes.Length; i++)
                if (bytes[i] != otherBytes[i])
                    return false;

            return true;
        }
    }
}
