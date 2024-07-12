using System;
using System.Threading.Tasks;

namespace NopStation.Plugin.Misc.TinyPNG.Services
{
    public  interface ITinyPNGService
    {
        Task<(byte[] imageByte, bool isCompressed)> TinifyImageAsync(byte[] sourceImg);
    }
}
