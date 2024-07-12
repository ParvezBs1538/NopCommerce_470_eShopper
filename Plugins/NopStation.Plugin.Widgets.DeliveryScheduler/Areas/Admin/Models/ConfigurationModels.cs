using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            CategorySearchModel = new CategorySearchModel();
        }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.Configuration.Fields.EnableScheduling")]
        public bool EnableScheduling { get; set; }
        public bool EnableScheduling_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.Configuration.Fields.NumberOfDaysToDisplay")]
        public int NumberOfDaysToDisplay { get; set; }
        public bool NumberOfDaysToDisplay_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.Configuration.Fields.DisplayDayOffset")]
        public int DisplayDayOffset { get; set; }
        public bool DisplayDayOffset_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.Configuration.Fields.DateFormat")]
        public string DateFormat { get; set; }
        public bool DateFormat_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.Configuration.Fields.ShowRemainingCapacity")]
        public bool ShowRemainingCapacity { get; set; }
        public bool ShowRemainingCapacity_OverrideForStore { get; set; }

        public CategorySearchModel CategorySearchModel { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}