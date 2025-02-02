﻿using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class ProductTagsModel
    {
        public ProductTagsModel()
        {
            CheckedState = CheckedState.UnChecked;
        }

        public int Count { get; set; }

        public CheckedState CheckedState { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }
    }
}
