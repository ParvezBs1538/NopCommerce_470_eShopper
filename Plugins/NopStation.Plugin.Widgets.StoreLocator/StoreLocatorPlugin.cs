using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.StoreLocator.Helpers;
using NopStation.Plugin.Widgets.StoreLocator.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Shipping.Tracking;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using System;
using NopStation.Plugin.Widgets.StoreLocator.Components;
using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.Widgets.StoreLocator
{
    public class StoreLocatorPlugin : BasePlugin, IPickupPointProvider, IAdminMenuPlugin, INopStationPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IAddressService _addressService;
        private readonly IStoreLocationService _storeLocationService;
        private readonly IStoreContext _storeContext;
        private readonly StoreLocatorSettings _storeLocatorSettings;

        #endregion

        #region Ctor

        public StoreLocatorPlugin(ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IAddressService addressService,
            IStoreLocationService storeLocationService,
            IStoreContext storeContext,
            StoreLocatorSettings storeLocatorSettings)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _addressService = addressService;
            _storeLocationService = storeLocationService;
            _storeContext = storeContext;
            _storeLocatorSettings = storeLocatorSettings;
        }

        #endregion

        #region Properties

        public bool HideInWidgetList => true;

        #endregion

        #region Methods

        public Task<IShipmentTracker> GetShipmentTrackerAsync()
        {
            return Task.FromResult<IShipmentTracker>(null);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/StoreLocator/Configure";
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.StoreLocator.Menu.StoreLocator"),
                IconClass = "far fa-dot-circle",
                Visible = true
            };

            if (await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocatorConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.StoreLocator.Menu.Configuration"),
                    Url = "~/Admin/StoreLocator/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "StoreLocator.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
            {
                var stores = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.StoreLocator.Menu.Stores"),
                    Url = "~/Admin/StoreLocation/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "StoreLocations"
                };
                menuItem.ChildNodes.Add(stores);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/store-locator-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=store-locator",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            var settings = new StoreLocatorSettings()
            {
                PictureSize = 500,
                PublicDispalyPageSize = 10,
                DistanceCalculationMethodId = (int)DistanceCalculationMethod.GeoCoordinate,
                FooterColumnSelector = ".footer .footer-block.information ul.list",
                HideInMobileView = false,
                IncludeInFooterColumn = true,
                IncludeInTopMenu = true,
                SortPickupPointsByDistance = true
            };
            await _settingService.SaveSettingAsync(settings);

            await NopPlugin.EnablePlugin(this, PluginType.WidgetPlugin);
            await this.InstallPluginAsync(new StoreLocatorPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await NopPlugin.DisablePlugin(this, PluginType.WidgetPlugin);
            await this.UninstallPluginAsync(new StoreLocatorPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new("Admin.NopStation.StoreLocator.Menu.StoreLocator", "Store locator"),
                new("Admin.NopStation.StoreLocator.Menu.Stores", "Stores"),
                new("Admin.NopStation.StoreLocator.Menu.Configuration", "Configuration"),

                new("Admin.NopStation.StoreLocator.Configuration", "Store locator settings"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.GoogleMapApiKey", "Google map api key"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.SortPickupPointsByDistance", "Sort pick-up points by distance"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.DistanceCalculationMethodId", "Distance calculation method"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.GoogleDistanceMatrixApiKey", "Google distance matrixapi key"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.IncludeInTopMenu", "Include in top menu"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.HideInMobileView", "Hide in mobile view"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.IncludeInFooterColumn", "Include in footer column"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.FooterColumnSelector", "Footer column selector"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.PublicDispalyPageSize", "Public dispaly page size"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.PictureSize", "Picture size"),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.GoogleMapApiKey.Hint", "The Google map api key. Click https://developers.google.com/maps/documentation/javascript/get-api-key to find more in details."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.SortPickupPointsByDistance.Hint", "This will determine whether pick-up points to be sorted by distance in checkout step or not."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.DistanceCalculationMethodId.Hint", "Select distance calculation method."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.GoogleDistanceMatrixApiKey.Hint", "The Google distance matrixapi key. Click https://developers.google.com/maps/documentation/distance-matrix/overview to find more in details."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.IncludeInTopMenu.Hint", "This will determine whether \"Find stores\" menu displayed in top menu or not."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.HideInMobileView.Hint", "Check to hide this menu in mobile view."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.IncludeInFooterColumn.Hint", "This will determine whether \"Find stores\" menu displayed in footer column or not."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.FooterColumnSelector.Hint", "The footer column selector."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.PublicDispalyPageSize.Hint", "This will determine how many stores will be displayed in a page in public store."),
                new("Admin.NopStation.StoreLocator.Configuration.Fields.PictureSize.Hint", "The store thumbnail picture size."),

                new("Admin.NopStation.StoreLocator.Configuration.GoogleDistanceMatrix.Hint", "Distance will be calculated by Google distance matrix and will sort by the actual distance by road. This is a paid service by Google. Click <a href=\"https://developers.google.com/maps/documentation/distance-matrix/overview\">here</a> to get more information about it."),
                new("Admin.NopStation.StoreLocator.Configuration.GeoCoordinate.Hint", "Represents a distance geographical location that is determined by latitude and longitude coordinates."),

                new("Admin.NopStation.StoreLocator.Configuration.BlockTitle.Map", "Map"),
                new("Admin.NopStation.StoreLocator.Configuration.BlockTitle.Menu", "Menu"),
                new("Admin.NopStation.StoreLocator.Configuration.BlockTitle.Seo", "Seo"),

                new("Admin.NopStation.StoreLocator.StoreLocations.Created", "Store location has been created successfully."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Updated", "Store location has been updated successfully."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Deleted", "Store location has been deleted successfully."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Name", "Name"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Name.Hint", "The store name."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.ShortDescription", "Short description"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.ShortDescription.Hint", "Specify short description of the store location."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.FullDescription", "Full description"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.FullDescription.Hint", "Specify full description of the store location."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Latitude", "Latitude"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Latitude.Hint", "The store latitude."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Longitude", "Longitude"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Longitude.Hint", "The store longitude."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.OpeningHours", "Opening hours"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.OpeningHours.Hint", "The store opening hours."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.GoogleMapLocation", "Google map location"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.GoogleMapLocation.Hint", "The store location on google map."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.FullAddress", "Full address"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.FullAddress.Hint", "The full address for this store."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Picture", "Picture"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Active", "Active"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Active.Hint", "This will determine whether the store is active or not."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.IsPickupPoint", "Is pickup point"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.IsPickupPoint.Hint", "This will determine whether the store will be marked as pickup point or not."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.PickupFee", "Pick-up fee"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.PickupFee.Hint", "Enter pick-up fee for this store."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.DisplayOrder", "Display order"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.DisplayOrder.Hint", "The store display order. 1 represents the first item in the list."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.LimitedToStores", "Limited to stores"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.LimitedToStores.Hint", "Option to limit this store location to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),

                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaDescription", "Meta description"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaDescription.Hint", "Meta description to be added to store location page header."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaKeywords", "Meta keywords"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaKeywords.Hint", "Meta keywords to be added to store location page header."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaTitle", "Meta keywords"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaTitle.Hint", "Override the page title. The default is the name of the store location."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.SeName", "Search engine friendly page name"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.SeName.Hint", "Set a search engine friendly page name e.g. 'the-best-store-location' to make your page URL 'http://www.yourStore.com/the-best-store-location'. Leave empty to generate it automatically based on the name of the store location."),

                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.FullAddress.Required", "The 'Full address'  is required."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.ShortDescription.Required", "The 'Short description'  is required."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Fields.Name.Required", "The 'Name'  is required."),

                new("Admin.NopStation.StoreLocator.StoreLocations.EditDetails", "Edit store details"),
                new("Admin.NopStation.StoreLocator.StoreLocations.BackToList", "back to store list"),
                new("Admin.NopStation.StoreLocator.StoreLocations.AddNew", "Add a new store"),

                new("Admin.NopStation.StoreLocator.StoreLocations.BlockTitle.Info", "Info"),
                new("Admin.NopStation.StoreLocator.StoreLocations.BlockTitle.Location", "Location"),
                new("Admin.NopStation.StoreLocator.StoreLocations.BlockTitle.Pictures", "Pictures"),

                new("Admin.NopStation.StoreLocator.StoreLocations.List", "Store locations"),
                new("Admin.NopStation.StoreLocator.StoreLocations.List.SearchStoreName", "Store name"),
                new("Admin.NopStation.StoreLocator.StoreLocations.List.SearchStoreName.Hint", "Search by a store name."),
                new("Admin.NopStation.StoreLocator.StoreLocations.List.SearchStore", "Store"),
                new("Admin.NopStation.StoreLocator.StoreLocations.List.SearchStore.Hint", "Search by a specific store."),

                new("NopStation.StoreLocator.FindStores", "Find stores"),
                new("NopStation.StoreLocator.NoPickupPoints", "No pickup points are available"),

                new("NopStation.StoreLocator.NoStores", "No stroes are available"),
                new("NopStation.StoreLocator.StoreLocations.Title", "Store locations"),
                new("NopStation.StoreLocator.StoreLocations.TotalStores", "Total stores ({0})"),
                new("NopStation.StoreLocator.StoreLocations.ShowInMap", "Show in map"),
                new("NopStation.StoreLocator.BackToAllShops", "Back To All Stores"),

                new("NopStation.StoreLocator.MapMarker.PhoneNumber", "Phone"),
                new("NopStation.StoreLocator.MapMarker.OpeningHours", "Opening Hours"),
                new("NopStation.StoreLocator.MapMarker.Address", "Address"),
                new("NopStation.StoreLocator.MapMarker.SeeMore", "See more"),
                new("NopStation.StoreLocator.MapMarker.SeeLess", "See less"),
                new("NopStation.StoreLocator.MapMarker.DefaultPosition", "Default Position"),

                new("NopStation.StoreLocator.ImageAlternateTextFormat.Details", "Picture of {0}"),
                new("NopStation.StoreLocator.ImageLinkTitleFormat.Details", "Picture of {0}"),
                new("NopStation.StoreLocator.ImageAlternateTextFormat", "Show details for {0}"),
                new("NopStation.StoreLocator.ImageLinkTitleFormat", "Show details for {0}"),

                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.Picture", "Picture"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.Picture.Hint", "Choose a picture to upload. If the picture size exceeds your stores max image size setting, it will be automatically resized."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.DisplayOrder", "Display order"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.DisplayOrder.Hint", "Display order of the picture. 1 represents the top of the list."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.OverrideAltAttribute", "Alt"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.OverrideAltAttribute.Hint", "Override \"alt\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. store location name)."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.OverrideTitleAttribute", "Title"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.OverrideTitleAttribute.Hint", "Override \"title\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. store location name)."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.AddNew", "Add a new picture"),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.SaveBeforeEdit", "You need to save the store location before you can upload pictures for this store location page."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Alert.AddNew", "Upload picture first."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Alert.PictureAdd", "Failed to add store location picture."),
                new("Admin.NopStation.StoreLocator.StoreLocations.Pictures.AddButton", "Add store location picture")
            };

            return list;
        }

        public async Task<GetPickupPointsResponse> GetPickupPointsAsync(IList<ShoppingCartItem> cart, Address address)
        {
            var result = new GetPickupPointsResponse();
            var pickupPoints = await _storeLocationService.SearchStoreLocationsAsync(
                active: true,
                pickupPoint: true,
                storeId: _storeContext.GetCurrentStore().Id);

            foreach (var storeLocation in await pickupPoints.OrderStoresAsync(_storeLocatorSettings))
            {
                var storeAddress = await _addressService.GetAddressByIdAsync(storeLocation.AddressId);
                if (storeAddress == null)
                    continue;

                var country = await _countryService.GetCountryByAddressAsync(storeAddress);
                var stateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(storeAddress);

                result.PickupPoints.Add(new PickupPoint
                {
                    Id = storeLocation.Id.ToString(),
                    Name = storeLocation.Name,
                    Description = storeLocation.FullDescription,
                    Address = storeAddress.Address1,
                    City = storeAddress.City,
                    County = storeAddress.County,
                    StateAbbreviation = stateProvince?.Abbreviation,
                    CountryCode = country?.TwoLetterIsoCode,
                    ZipPostalCode = storeAddress.ZipPostalCode,
                    OpeningHours = storeLocation.OpeningHours,
                    PickupFee = storeLocation.PickupFee,
                    DisplayOrder = storeLocation.DisplayOrder,
                    ProviderSystemName = PluginDescriptor.SystemName
                });
            }

            if (!result.PickupPoints.Any())
                result.AddError(await _localizationService.GetResourceAsync("NopStation.StoreLocator.NoPickupPoints"));

            return result;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>()
            {
                PublicWidgetZones.HeaderMenuAfter,
                PublicWidgetZones.MobHeaderMenuAfter,
                PublicWidgetZones.Footer
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if(widgetZone != PublicWidgetZones.Footer)
                return typeof(StoreLocatorTopMenuViewComponent);

            return typeof(StoreLocatorFooterViewComponent);
        }

        #endregion
    }
}
