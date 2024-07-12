using System;
using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.ProductRequests
{
    public class ProductRequestSettings: ISettings
    {
        public ProductRequestSettings()
        {
            AllowedCustomerRolesIds = new List<int>();
        }

        public List<int> AllowedCustomerRolesIds { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public bool IncludeInFooter { get; set; }

        public string FooterElementSelector { get; set; }

        public bool LinkRequired { get; set; }

        public bool DescriptionRequired { get; set; }

        public int MinimumProductRequestCreateInterval { get; set; }
    }
}
