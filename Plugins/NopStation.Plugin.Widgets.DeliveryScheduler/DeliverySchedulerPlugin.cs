using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Components;
using NopStation.Plugin.Widgets.DeliveryScheduler.Components;

namespace NopStation.Plugin.Widgets.DeliveryScheduler
{
    public class DeliverySchedulerPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        public bool HideInWidgetList => false;

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public DeliverySchedulerPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/DeliveryScheduler/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.CheckoutShippingMethodBottom || widgetZone == PublicWidgetZones.OpCheckoutShippingMethodBottom)
                return typeof(DeliverySchedulerViewComponent);
            if (widgetZone == PublicWidgetZones.OrderDetailsPageOverview)
                return typeof(OrderDeliverySlotPublicViewComponent);

            return typeof(OrderDeliverySlotViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.CheckoutShippingMethodBottom,
                PublicWidgetZones.OpCheckoutShippingMethodBottom,
                AdminWidgetZones.OrderDetailsBlock,
                PublicWidgetZones.OrderDetailsPageOverview
            });
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.Menu.DeliveryScheduler"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.Menu.Configuration"),
                    Url = "~/Admin/DeliveryScheduler/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DeliveryScheduler.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliverySlots))
            {
                var manageDeliverySlot = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.Menu.DeliverySlots"),
                    Url = "~/Admin/DeliverySlot/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DeliveryScheduler.DeliverySlots"
                };
                menuItem.ChildNodes.Add(manageDeliverySlot);
            }
            if (await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
            {
                var manageDeliveryDateSlot = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.Menu.DeliveryCapacities"),
                    Url = "~/Admin/DeliveryCapacity/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DeliveryScheduler.DeliveryCapacities"
                };
                menuItem.ChildNodes.Add(manageDeliveryDateSlot);

                var manageSpecialDateSlot = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.Menu.SpecialDeliveryCapacities"),
                    Url = "~/Admin/SpecialDeliveryCapacity/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DeliveryScheduler.SpecialDeliveryCapacities"
                };
                menuItem.ChildNodes.Add(manageSpecialDateSlot);
            }
            if (await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ViewOrderDeliveryInfo))
            {
                var manageOrderDeliveryList = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.Menu.OrderList"),
                    Url = "~/Admin/OrderDeliverySlot/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DeliveryScheduler.OrderList"
                };
                menuItem.ChildNodes.Add(manageOrderDeliveryList);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/delivery-scheduler-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=delivery-scheduler",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            var setting = new DeliverySchedulerSettings()
            {
                EnableScheduling = true,
                DisplayDayOffset = 0,
                NumberOfDaysToDisplay = 7,
                DateFormat = "dddd MMM dd, yyyy"
            };
            await _settingService.SaveSettingAsync(setting);

            await this.InstallPluginAsync(new DeliverySchedulerPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new DeliverySchedulerPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                // menu
                new("Admin.NopStation.DeliveryScheduler.Menu.DeliveryScheduler", "Delivery scheduler"),
                new("Admin.NopStation.DeliveryScheduler.Menu.Configuration", "Configuration"),
                new("Admin.NopStation.DeliveryScheduler.Menu.DeliverySlots", "Delivery slots"),
                new("Admin.NopStation.DeliveryScheduler.Menu.DeliveryCapacities", "Delivery capacities"),
                new("Admin.NopStation.DeliveryScheduler.Menu.SpecialDeliveryOffsets", "Special delivery offsets"),
                new("Admin.nopstation.DeliveryScheduler.Menu.OrderList", "Orders"),
                new("Admin.NopStation.DeliveryScheduler.Menu.SpecialDeliveryCapacities", "Special delivery capacities"),

                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.NoShippingMethod", "No shipping method found"),

                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Created", "Delivery slot has been created successfully."),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Deleted", "Delivery slot has been deleted successfully."),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Updated", "Delivery slot has been updated successfully."),

                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.ShippingMethod", "Shipping method"),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.ShippingMethod.Hint", "The shipping method."),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.DeliverySlot", "Delivery slot"),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day1Capacity", "Sunday"),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day2Capacity", "Monday"),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day3Capacity", "Tuesday"),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day4Capacity", "Wednesday"),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day5Capacity", "Thursday"),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day6Capacity", "Friday"),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day7Capacity", "Saturday"),
                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.DaysOfWeek", "Days of week"),

                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Created", "Special delivery capacities has been created successfully."),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Deleted", "Special delivery capacities has been deleted successfully."),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Updated", "Special delivery capacities has been updated successfully."),

                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.List", "Special capacities"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.AddNew", "Add new special capacities"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.BackToList", "back to special capacity list"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.EditDetails", "Edit special capacity details"),

                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.SpecialDate", "Special date"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.DeliverySlot", "Delivery slot"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.Capacity", "Capacity"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.MappedStoreNames", "Mapped Store Names"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.Note", "Note"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.LimitedToStores", "Limited To Stores"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.SpecialDate.Hint", "Select special date."),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.ShippingMethod", "Shipping method"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.ShippingMethod.Hint", "Select shipping method."),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.DeliverySlot.Hint", "The delivery slot."),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.Capacity.Hint", "Capacity for the special date"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.Note.Hint", "Note for special capacity."),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.LimitedToStores.Hint", "Select the stores you want to limit it to"),

                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.LimitedToStores", "Limited to stores"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.LimitedToStores.Hint", "Option to limit this flipbook to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.TimeSlot", "Time slot"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.DisplayOrder", "Display order"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.Active", "Active"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.CreatedOn", "Created on"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.LimitedToShippingMethods", "Limited to shipping methods"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.TimeSlot.Hint", "The time slot."),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.DisplayOrder.Hint", "The display order for this delivery slot. 1 represents the top of the list."),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.Active.Hint", "Defines whether slot is active or not."),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.CreatedOn.Hint", "The create date."),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.LimitedToShippingMethods.Hint", "Option to limit this delivery slot to a certain shipping method. If you have multiple shipping methods, choose one or several from the list. If you don't use this option just leave this field empty."),

                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryOffsets.Fields.DaysOffset", "Days offset"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryOffsets.Fields.CategoryName", "Category"),

                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities", "Delivery capacity"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.AddNew", "Add new delivery slot"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.EditDetails", "Edit delivery slot details"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.BackToList", "back to delivery slot list"),
                new("Admin.NopStation.DeliveryScheduler.DeliverySlots.List", "Delivery slots"),

                new("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Updated", "Delivery capacity updated successfully."),

                new("Admin.NopStation.DeliveryScheduler.Configuration", "Delivery scheduler settings"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.NumberOfDaysToDisplay", "Number of days to display"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.DisplayDayOffset", "Display day offset"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.EnableScheduling", "Enable scheduling"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.DateFormat", "Date format"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.ShowRemainingCapacity", "Show remaining capacity"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.NumberOfDaysToDisplay.Hint", "Number of days to display"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.DisplayDayOffset.Hint", "Display day offset"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.EnableScheduling.Hint", "Enable scheduling"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.DateFormat.Hint", "The date format to display in checkout page."),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.ShowRemainingCapacity.Hint", "Show remaining capacity in checkout page."),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Fields.DateFormat.Sample", "(i.e. {0})"),

                new("Admin.NopStation.DeliveryScheduler.Configuration.OffsetOverride", "Custom offset days"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Settings", "Settings"),
                new("Admin.NopStation.DeliveryScheduler.Configuration.Button.Reset", "Reset"),
                new("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.AlreadyOverridden", "Capacity has been already overridden for this date and slot."),

                new("NopStation.DeliveryScheduler.Slots.SlotsAvailable", "{0} slots available"),
                new("NopStation.DeliveryScheduler.Slots.SlotAvailable", "{0} slot available"),
                new("NopStation.DeliveryScheduler.Slots.Booked", "All Booked"),
                new("NopStation.DeliveryScheduler.Slots.NotSelected", "Please select a delivery slot"),

                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.DeliverySlot", "Delivery slot"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.DeliveryDate", "Delivery date"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.ShippingMethod", "Shipping method"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.DeliverySlot.Hint", "The order delivery slot."),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.DeliveryDate.Hint", "The order delivery date."),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.ShippingMethod.Hint", "The order shipping method."),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.DeliverySlot.All", "All"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.ShippingMethod.All", "All"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Updated", "Order delivery slot updated successfully."),

                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Info", "Delivery info"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List", "Orders"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchShippingMethod", "Shipping method"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchShippingMethod.Hint", "Search by shipping method."),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchDeliverySlot", "Delivery slot"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchDeliverySlot.Hint", "Search by delivery slot."),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchStartDate", "From date"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchStartDate.Hint", "Select from date."),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchEndTime", "To date"),
                new("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchEndTime.Hint", "Select to date."),

                new("NopStation.DeliveryScheduler.OrderDeliverySlot.Fields.TimeSlot", "Delivery Slot"),
                new("NopStation.DeliveryScheduler.OrderDeliverySlot.Fields.ShippingDate", "Shipping Date"),

            };

            return list;
        }

        #endregion
    }
}
