﻿using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models
{
    public record ProductTabItemSearchModel : BaseSearchModel
    {
        public int ProductTabId { get; set; }
    }
}