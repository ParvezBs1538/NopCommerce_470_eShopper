using System.Collections.Generic;
using System.Linq;

namespace System;

public static class ProductBadgeHelper
{
    public static List<int> ToIntList(this string val)
    {
        if (string.IsNullOrWhiteSpace(val))
            return new List<int>();

        var arr = val.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
        return arr.Select(int.Parse).ToList();
    }

    public static string ToFormattedString(this IList<int> list)
    {
        if (list == null || !list.Any())
            return string.Empty;

        return string.Join(',', list);
    }
}