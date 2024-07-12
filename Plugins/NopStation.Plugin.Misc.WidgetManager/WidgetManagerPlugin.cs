using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Common;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.WidgetManager;

public class WidgetManagerPlugin : BasePlugin, IMiscPlugin, INopStationPlugin
{
    #region Methods

    public override async Task InstallAsync()
    {
        await this.InstallPluginAsync();
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync();
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.WidgetManager.Common.Guest"] = "Guest",
            ["Admin.NopStation.WidgetManager.Common.WidgetZones"] = "Widget zones",
            ["Admin.NopStation.WidgetManager.Common.Schedules"] = "Schedules",
            ["Admin.NopStation.WidgetManager.Common.CustomerConditions"] = "Customer conditions",
            ["Admin.NopStation.WidgetManager.Common.ProductConditions"] = "Product conditions",
            ["Admin.NopStation.WidgetManager.WidgetZones.AddNew"] = "Add new widget zone",
            ["Admin.NopStation.WidgetManager.WidgetZones.AddButton"] = "Add widget zone",
            ["Admin.NopStation.WidgetManager.WidgetZones.SaveBeforeEdit"] = "You need to save the entity before you can add widget zone mappings for this page.",
            ["Admin.NopStation.WidgetManager.WidgetZones.Alert.AddNew"] = "Select widget zone first.",
            ["Admin.NopStation.WidgetManager.WidgetZones.Alert.WidgetZoneAdd"] = "Failed to add widget zone.",

            ["Admin.NopStation.WidgetManager.Schedules.Fields.AvaliableDateTimeFromUtc"] = "Avaliable date time UTC",
            ["Admin.NopStation.WidgetManager.Schedules.Fields.AvaliableDateTimeFromUtc.Hint"] = "Select avaliable date time in UTC.",
            ["Admin.NopStation.WidgetManager.Schedules.To"] = "- to -",
            ["Admin.NopStation.WidgetManager.Schedules.Fields.ScheduleType"] = "Schedule type",
            ["Admin.NopStation.WidgetManager.Schedules.Fields.ScheduleType.Hint"] = "Select schedule type.",
            ["Admin.NopStation.WidgetManager.Schedules.Fields.TimeOfDayFromUtc"] = "Time of day UTC",
            ["Admin.NopStation.WidgetManager.Schedules.Fields.TimeOfDayFromUtc.Hint"] = "Select availability time range in UTC of a day.",
            ["Admin.NopStation.WidgetManager.Schedules.Fields.SelectedDaysOfWeek"] = "Days",
            ["Admin.NopStation.WidgetManager.Schedules.Fields.SelectedDaysOfWeek.Hint"] = "Select days of week.",
            ["Admin.NopStation.WidgetManager.Schedules.Fields.SelectedDaysOfMonth"] = "Days",
            ["Admin.NopStation.WidgetManager.Schedules.Fields.SelectedDaysOfMonth.Hint"] = "Select days of month.",

            ["Admin.NopStation.WidgetManager.WidgetZones.Fields.WidgetZone"] = "Widget zone",
            ["Admin.NopStation.WidgetManager.WidgetZones.Fields.WidgetZone.Hint"] = "The widget zone.",
            ["Admin.NopStation.WidgetManager.WidgetZones.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.WidgetManager.WidgetZones.Fields.DisplayOrder.Hint"] = "The display order of the widget zone mapping. 1 represents the first item in widget zone mapping list.",

            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.CustomerRoles"] = "Customer roles",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.CustomerRoles.Hint"] = "Filter by customer role.",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchCompany"] = "Company",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchCompany.Hint"] = "Search by company.",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchDateOfBirth"] = "Date of birth",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchDateOfBirth.Day"] = "Day",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchDateOfBirth.Hint"] = "Filter by date of birth. Don't select any value to load all records.",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchDateOfBirth.Month"] = "Month",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchEmail"] = "Email",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchEmail.Hint"] = "Search by a specific email.",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchFirstName"] = "First name",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchFirstName.Hint"] = "Search by a first name.",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchIpAddress"] = "IP address",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchIpAddress.Hint"] = "Search by IP address.",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchLastName"] = "Last name",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchLastName.Hint"] = "Search by a last name.",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchPhone"] = "Phone",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchPhone.Hint"] = "Search by a phone number.",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchUsername"] = "Username",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchUsername.Hint"] = "Search by a specific username.",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchZipCode"] = "Zip code",
            ["Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchZipCode.Hint"] = "Search by zip code.",

            ["Admin.NopStation.WidgetManager.Conditions.Customers.AddNew"] = "Add new customer",
            ["Admin.NopStation.WidgetManager.CustomerConditions.AddNew"] = "Add new",
            ["Admin.NopStation.WidgetManager.CustomerConditions.Fields.Customer"] = "Customer",
            ["Admin.NopStation.WidgetManager.CustomerConditions.Fields.Active"] = "Active",
            ["Admin.NopStation.WidgetManager.CustomerConditions.SaveBeforeEdit"] = "You need to save the entity before you can add customer condition mappings for this page.",

            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchCategory"] = "Category",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchCategory.Hint"] = "Search by a specific category.",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchIncludeSubCategories"] = "Search subcategories",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchIncludeSubCategories.Hint"] = "Check to search in subcategories.",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchManufacturer"] = "Manufacturer",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchManufacturer.Hint"] = "Search by a specific manufacturer.",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchProductName"] = "Product name",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchProductName.Hint"] = "A product name.",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchProductType"] = "Product type",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchProductType.Hint"] = "Search by a product type.",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchPublished"] = "Published",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchPublished.All"] = "All",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchPublished.Hint"] = "Search by a \"Published\" property.",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchPublished.PublishedOnly"] = "Published only",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchPublished.UnpublishedOnly"] = "Unpublished only",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchStore"] = "Store",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchStore.Hint"] = "Search by a specific store.",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchVendor"] = "Vendor",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchVendor.Hint"] = "Search by a specific vendor.",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchWarehouse"] = "Warehouse",
            ["Admin.NopStation.WidgetManager.Conditions.Products.List.SearchWarehouse.Hint"] = "Search by a specific warehouse.",

            ["Admin.NopStation.WidgetManager.Conditions.Products.AddNew"] = "Add new product",
            ["Admin.NopStation.WidgetManager.ProductConditions.AddNew"] = "Add new",
            ["Admin.NopStation.WidgetManager.ProductConditions.Fields.Product"] = "Product",
            ["Admin.NopStation.WidgetManager.ProductConditions.Fields.Published"] = "Published",
            ["Admin.NopStation.WidgetManager.ProductConditions.SaveBeforeEdit"] = "You need to save the entity before you can add product condition mappings for this page.",
        };

        return list.ToList();
    }

    #endregion
}
