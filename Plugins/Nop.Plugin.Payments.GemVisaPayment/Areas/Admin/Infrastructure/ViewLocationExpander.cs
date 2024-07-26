﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Payments.GemVisaPayment.Areas.Admin.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin")
            {
                viewLocations = new[] {
                    $"/Plugins/Payments.GemVisaPayment/Areas/Admin/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/Payments.GemVisaPayment/Areas/Admin/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                    $"/Plugins/Payments.GemVisaPayment/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/Payments.GemVisaPayment/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);

            }

            return viewLocations;
        }
    }
}