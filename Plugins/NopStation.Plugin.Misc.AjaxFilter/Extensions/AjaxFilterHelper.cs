using System;
using System.Collections.Generic;
using System.Linq;

namespace NopStation.Plugin.Misc.AjaxFilter.Extensions
{
    public static class AjaxFilterHelper
    {
        public static List<int> ConvertToIntegerList(string commaSeperatedIntegerString)
        {
            var ids = new List<int>();

            if (string.IsNullOrEmpty(commaSeperatedIntegerString))
                return ids;

            var rangeArray = commaSeperatedIntegerString
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToList();

            foreach (var str1 in rangeArray)
            {
                if (int.TryParse(str1, out var tmp1))
                    ids.Add(tmp1);
            }

            return ids;
        }
    }
}
