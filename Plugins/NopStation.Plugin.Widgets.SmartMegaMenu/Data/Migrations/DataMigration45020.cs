using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.SmartMegaMenu.Data.Migrations.OldDomain;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Data.Migrations;

[NopMigration("2022-12-29 00:00:00", "4.50.2.0", UpdateMigrationType.Data, MigrationProcessType.Update)]
public class DataMigration45020 : Migration
{
    public static string MenuItemSavePrefix => "Nopstation.SmartMegaMenu.MenuItems.{0}";

    private readonly INopDataProvider _dataProvider;
    private readonly ILocalizationService _localizationService;
    private readonly INopStationPluginManager _nopStationPluginManager;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWidgetZoneService _widgetZoneService;
    private readonly ISettingService _settingService;

    public DataMigration45020(INopDataProvider dataProvider,
        ILocalizationService localizationService,
        INopStationPluginManager nopStationPluginManager,
        IGenericAttributeService genericAttributeService,
        IWidgetZoneService widgetZoneService,
        ISettingService settingService)
    {
        _dataProvider = dataProvider;
        _localizationService = localizationService;
        _nopStationPluginManager = nopStationPluginManager;
        _genericAttributeService = genericAttributeService;
        _widgetZoneService = widgetZoneService;
        _settingService = settingService;
    }

    public override void Up()
    {
        //locales
        _localizationService.DeleteLocaleResourcesAsync("Admin.NopStation.Plugin.Widgets.SmartMegaMenu").Wait();

        var plugin = _nopStationPluginManager.LoadNopStationPluginsAsync(pluginSystemName: "NopStation.Plugin.Widgets.SmartMegaMenu").Result.FirstOrDefault();
        foreach (var keyValuePair in plugin.PluginResouces())
            _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value).Wait();

        foreach (var megaMenu in _dataProvider.GetTable<MegaMenu>().ToList())
        {
            var saveKey = string.Format(MenuItemSavePrefix, megaMenu.Id);

            var keyGroup = "Menu";

            var props = _genericAttributeService.GetAttributesForEntityAsync(megaMenu.Id, keyGroup).Result;

            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(saveKey, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            if (prop == null)
                continue;

            var menuItemData = CommonHelper.To<string>(prop.Value);

            if (!string.IsNullOrEmpty(menuItemData))
            {
                var i = 0;
                var menuItems = JsonConvert.DeserializeObject<List<MenuItem>>(menuItemData);
                var allMenuItems = new Dictionary<MegaMenuItem, MenuItem>();

                foreach (var item in menuItems)
                {
                    var menuType = GetMenuItemType(item);

                    var mi = new MegaMenuItem
                    {
                        SubjectToAcl = false,
                        CategoryId = menuType.Item2,
                        DisplayOrder = i++,
                        PageTypeId = menuType.Item2,
                        MegaMenuId = megaMenu.Id,
                        PictureId = item.PictureId,
                        RibbonBackgroundColor = item.RibbonColor,
                        RibbonTextColor = item.RibbonTextColor,
                        RibbonText = item.RibbonText,
                        ShowRibbonText = !string.IsNullOrWhiteSpace(item.RibbonText),
                        ShowPicture = item.PictureId > 0,
                        Title = item.Title,
                        Url = item.Url,
                        OpenInNewTab = false,
                        MenuItemType = menuType.Item1,
                        ManufacturerId = menuType.Item2,
                        ProductTagId = menuType.Item2,
                        TopicId = menuType.Item2,
                        VendorId = menuType.Item2
                    };
                    _dataProvider.InsertEntity(mi);

                    allMenuItems.Add(mi, item);
                }

                foreach (var item in allMenuItems)
                {
                    MegaMenuItem parentItem = null;
                    foreach (var item1 in allMenuItems)
                    {
                        if (item1.Key == item.Key)
                            continue;

                        var menuItemOld = item1.Value;
                        if (menuItemOld.MenuItemId == item.Value.ParentItem)
                        {
                            parentItem = item1.Key;
                            break;
                        }
                    }

                    if (parentItem == null)
                        continue;

                    item.Key.ParentMenuItemId = parentItem.Id;
                }

                _dataProvider.UpdateEntitiesAsync(allMenuItems.Keys);
            }
        }

        foreach (var megaMenu in _dataProvider.GetTable<Menu>().ToList())
        {
            if (megaMenu.WidgetZoneId == 0)
                continue;

            var widgetZone = GetWidgetZone(megaMenu.WidgetZoneId);
            if (string.IsNullOrWhiteSpace(widgetZone))
                continue;

            var widgetZoneMapping = new WidgetZoneMapping
            {
                DisplayOrder = 0,
                EntityId = megaMenu.Id,
                WidgetZone = widgetZone,
                EntityName = "NS_SmartMegaMenu"
            };
            _widgetZoneService.InsertWidgetZoneMappingAsync(widgetZoneMapping).Wait();
        }

        var menuTableName = NameCompatibilityManager.GetTableName(typeof(MegaMenu));
        var widgetZoneIdColumnName = "WidgetZoneId";
        if (Schema.Table(menuTableName).Column(widgetZoneIdColumnName).Exists())
            Delete.Column(widgetZoneIdColumnName).FromTable(menuTableName);

        var megaMenuSettings = _settingService.LoadSettingAsync<SmartMegaMenuSettings>().Result;
        if (!_settingService.SettingExistsAsync(megaMenuSettings, settings => settings.HideDefaultMenu).Result)
        {
            megaMenuSettings.HideDefaultMenu = true;
            _settingService.SaveSettingAsync(megaMenuSettings, settings => settings.HideDefaultMenu).Wait();
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }

    private string GetWidgetZone(int widgetZoneId)
    {
        if (widgetZoneId == 4)
            return "left_category_menu";
        if (widgetZoneId == 5)
            return "right_header_link";
        return null;
    }

    private (MenuItemType, int) GetMenuItemType(MenuItem menuItem)
    {
        if (string.IsNullOrWhiteSpace(menuItem.MenuType))
            return (MenuItemType.CustomLink, 0);

        if (menuItem.MenuType.Contains("Category"))
            return (MenuItemType.Category, ParseInt(menuItem.MenuItemId));

        if (menuItem.MenuType.Contains("Manufacturer"))
            return (MenuItemType.Manufacturer, ParseInt(menuItem.MenuItemId));

        if (menuItem.MenuType.Contains("Vendor"))
            return (MenuItemType.Vendor, ParseInt(menuItem.MenuItemId));

        if (menuItem.MenuType.Contains("Page"))
            return (MenuItemType.Page, ParseInt(menuItem.MenuItemId));

        if (menuItem.MenuType.Contains("Topic"))
            return (MenuItemType.Topic, ParseInt(menuItem.MenuItemId));

        if (menuItem.MenuType.Contains("ProductTag"))
            return (MenuItemType.ProductTag, ParseInt(menuItem.MenuItemId));

        return (MenuItemType.CustomLink, 0);
    }

    private int ParseInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        var str = new string(value.Where(char.IsNumber).ToArray());
        return int.TryParse(str, out var intVal) ? intVal : 0;
    }
}
