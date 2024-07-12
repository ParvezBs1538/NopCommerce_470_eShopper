using System;
using System.Linq;

namespace NopStation.Plugin.Misc.Core;

public class CoreHelpers
{
    public static string RandomString(int length)
    {
        var random = new Random();
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789-";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
