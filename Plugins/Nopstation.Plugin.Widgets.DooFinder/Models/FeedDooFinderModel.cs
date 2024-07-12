using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nopstation.Plugin.Widgets.DooFinder.Models
{
    public record FeedDooFinderModel
    {
        public FeedDooFinderModel()
        {
            AvailableCurrencies = new List<SelectListItem>();
            GeneratedFiles = new List<GeneratedFileModel>();
            AvailableHours = new List<SelectListItem>();
            AvailableMinutes = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.ProductPictureSize")]
        public int ProductPictureSize { get; set; }
        public bool ProductPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.Currency")]
        public int CurrencyId { get; set; }
        public IList<SelectListItem> AvailableCurrencies { get; set; }
        public bool CurrencyId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.PricesConsiderPromotions")]
        public bool PricesConsiderPromotions { get; set; }
        public bool PricesConsiderPromotions_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.StaticFilePath")]
        public IList<GeneratedFileModel> GeneratedFiles { get; set; }

        public bool HideConfigBlock { get; set; }
        public bool HideSchedulerBlock { get; set; }
        public bool HideSettingsBlock { get; set; }

        public bool HideProductSettingsBlock { get; set; }

        //*********************** Schedule Feed Generating Time ***********************//
        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedGeneratingHour")]
        public int ScheduleFeedGeneratingHour { get; set; }
        public bool ScheduleFeedGeneratingHour_OverrideForStore { get; set; }
        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedGeneratingMinute")]
        public int ScheduleFeedGeneratingMinute { get; set; }
        public bool ScheduleFeedGeneratingMinute_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedLastExecutionStartTime")]
        public DateTime? ScheduleFeedLastExecutionStartTime { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedLastExecutionEndTime")]
        public DateTime? ScheduleFeedLastExecutionEndTime { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedNextExecutionTime")]
        public DateTime? ScheduleFeedNextExecutionTime { get; set; }

        public bool IsFeedGenerating { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.ApiScript")]
        public string ApiScript { get; set; }
        public bool ApiScript_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.AddAttributes")]
        public bool AddAttributes { get; set; }
        public bool AddAttributes_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nopstation.Plugin.Widgets.DooFinder.ActiveScript")]
        public bool ActiveScript { get; set; }
        public bool ActiveScript_OverrideForStore { get; set; }

        // *********************** Selection List ********************//
        public IList<SelectListItem> AvailableHours { get; set; }
        public IList<SelectListItem> AvailableMinutes { get; set; }
    }
}