using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance
{
    public static class PickupInStoreHelper
    {
        public static IList<SelectListItem> ToSelectList<TEnum>(this TEnum enumObj)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                         select new { Value = Convert.ToInt32(e), Text = e.ToString() };
            var list = values.Select(x =>
             {
                 var item = new SelectListItem
                 {
                     Text = x.Text,
                     Value = x.Value.ToString(),
                 };
                 return item;

             }).ToList();

            return list;
        }
    }
}
